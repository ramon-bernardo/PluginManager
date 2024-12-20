using System.Reflection;
using PluginManager.Plugins;

namespace PluginManager.Events;

internal sealed class EventManager
{
    private readonly IDictionary<string, IDictionary<EventPriority, IList<EventMethod>>> Events; // FIXME: list of methods

    internal EventManager()
    {
        Events = new Dictionary<string, IDictionary<EventPriority, IList<EventMethod>>>();
    }

    internal void Register(IPlugin plugin, string eventName, IEventListener listener, EventPriority priority, MethodInfo method)
    {
        if (!Events.TryGetValue(eventName, out var priorityEventMethods))
        {
            priorityEventMethods = new Dictionary<EventPriority, IList<EventMethod>>();
            Events.Add(eventName, priorityEventMethods);
        }

        if (!priorityEventMethods.TryGetValue(priority, out var methods))
        {
            methods = [];
            priorityEventMethods.Add(priority, methods);
        }

        if (!methods.Any(method => method.Plugin == plugin))
        {
            methods.Add(new EventMethod(plugin, listener, method));
        }
    }

    internal void Unregister(IPlugin plugin)
    {
        if (!Events.Any())
        {
            return;
        }

        foreach (var (eventName, priorityEventMethods) in Events.ToList())
        {
            foreach (var (priority, methods) in priorityEventMethods.ToList())
            {
                var filteredMethods = methods.Where(method => method.Plugin != plugin).ToList();
                if (filteredMethods.Any())
                {
                    priorityEventMethods[priority] = filteredMethods;
                }
                else
                {
                    priorityEventMethods.Remove(priority);
                }
            }

            if (!priorityEventMethods.Any())
            {
                Events.Remove(eventName);
            }
        }
    }

    internal T Send<T>(T e)
        where T : IEvent
    {
        var type = e.GetType();
        var eventName = type.Name;

        // Find callback methods associated with the event
        if (!Events.TryGetValue(eventName, out var eventMethods))
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
                method.Method.Invoke(method.Listener, [e]); // Failable, should be wrapped in a try-catch block
            }
            catch (Exception ex)
            {
                // TODO: handle this error
                Console.WriteLine(string.Format("Error invoking method '{0}' with event '{1}': {2}", method.Method.Name, e, ex.Message));
            }
        }

        return e;
    }
}
