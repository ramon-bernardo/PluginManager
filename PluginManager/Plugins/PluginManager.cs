using System.Reflection;
using PluginManager.Events;

namespace PluginManager.Plugins;

public sealed class PluginManager
{
    private readonly EventManager EventManager;

    private readonly IList<IPlugin> Plugins;

    public PluginManager()
    {
        EventManager = new EventManager(); // Ok, we can do better.

        Plugins = [];
    }

    public void LoadPlugin(IPlugin plugin)
    {
        if (Plugins.Contains(plugin))
        {
            // ignore if loaded.
            return;
        }

        try
        {
            plugin.OnLoad(this);
        }
        catch (Exception ex)
        {
            // TODO: handle this error
            Console.WriteLine(string.Format("Failed to load plugin ({0}): {1}", plugin.Name, ex.Message));
        }

        Plugins.Add(plugin);
    }

    public void UnloadPlugin(IPlugin plugin)
    {
        if (!Plugins.Contains(plugin))
        {
            // ignore if not loaded.
            return;
        }

        // Remove the registered events.
        EventManager.Unregister(plugin);

        try
        {
            plugin.OnUnload(this);
        }
        catch (Exception ex)
        {
            // TODO: handle this error
            Console.WriteLine(string.Format("Failed to unload plugin ({0}): {1}", plugin.Name, ex.Message));
        }

        Plugins.Remove(plugin);
    }

    public void UnloadPlugins()
    {
        while (Plugins.Any())
        {
            // Always remove the first plugin to avoid
            // modifying the collection during iteration
            var plugin = Plugins.First();
            UnloadPlugin(plugin);
        }
    }

    public IEnumerable<IPlugin> GetPlugins()
    {
        return Plugins;
    }

    public IPlugin? GetPlugin(string name)
    {
        return Plugins.FirstOrDefault(p => p.Name == name);
    }

    public bool Contains(IPlugin plugin)
    {
        return Plugins.Contains(plugin);
    }

    public void RegisterListener(IPlugin plugin, IEventListener listener)
    {
        var methods = listener.GetType()
                              .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) // TODO: doc.
                              .OfType<MethodInfo>();

        foreach (var method in methods)
        {
            var methodAttribute = method.GetCustomAttribute<Event>();
            if (methodAttribute == null)
            {
                continue;
            }

            var methodParameters = method.GetParameters();
            if (methodParameters.Length != 1)
            {
                // TODO: alert if the method does not have exactly one parameter
                continue;
            }

            var eventParameterType = methodParameters[0].ParameterType;
            if (!typeof(IEvent).IsAssignableFrom(eventParameterType))
            {
                continue;
            }

            var e = (IEvent?)Activator.CreateInstance(eventParameterType);
            if (e == null)
            {
                throw new InvalidOperationException(string.Format("Failed to create an instance of event type {0}.", eventParameterType.FullName));
            }

            EventManager.Register(plugin, e, methodAttribute.Priority, method);
        }
    }

    public IEvent SendEvent(IEvent e)
    {
        return EventManager.Send(e);
    }
}
