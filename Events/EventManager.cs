using System.Reflection;

namespace PluginManager.Events;

using EventMethodsPriority = IDictionary<EventPriority, IList<MethodInfo>>;

internal sealed class EventManager
{
    private readonly IDictionary<IEvent, EventMethodsPriority> Events;

    public EventManager()
    {
        Events = new Dictionary<IEvent, EventMethodsPriority>();
    }

    public void Register(IEvent e, EventPriority priority, MethodInfo method)
    {
        if (!Events.TryGetValue(e, out var priorityEventMethods))
        {
            priorityEventMethods = new Dictionary<EventPriority, IList<MethodInfo>>();
            Events.Add(e, priorityEventMethods);
        }

        if (!priorityEventMethods.TryGetValue(priority, out var methods))
        {
            methods = [];
            priorityEventMethods.Add(priority, methods);
        }

        methods.Add(method);
    }

    public void Unregister(IEvent e)
    {
        Events.Remove(e);
    }

    public T Send<T>(params object[] args)
        where T : IEvent
    {
        var type = typeof(T);

        var e = (T?)Activator.CreateInstance(type, args);
        if (e == null)
        {
            throw new InvalidOperationException(string.Format("Failed to create an instance of {0}", type.FullName));
        }

        return Send(e);
    }

    public T Send<T>(T e)
        where T : IEvent
    {
        // Find callback methods associated with the event
        if (!Events.TryGetValue(e, out var eventMethods))
        {
            return e;
        }

        var orderedPriorityMethods = eventMethods
            .OrderByDescending(method => method.Key) // Key: EventPriority
            .SelectMany(pair => pair.Value);

        foreach (var method in orderedPriorityMethods)
        {
            try
            {
                method.Invoke(this, [e]); // Failable, should be wrapped in a try-catch block
            }
            catch (Exception ex)
            {
                // TODO: handle this error
                Console.WriteLine(string.Format("Error invoking method '{0}' with event '{1}': {2}", method.Name, e, ex.Message));
            }
        }

        return e;
    }
}
