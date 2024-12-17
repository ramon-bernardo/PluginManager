using System.Reflection;
using PluginManager.Events;
using PluginManager.Events.Plugin;

namespace PluginManager.Plugins;

public sealed class PluginManager
{
    private readonly EventManager EventManager;

    private readonly IList<IPlugin> EnabledPlugins;
    private readonly IList<IPlugin> LoadedPlugins;

    public PluginManager()
    {
        EventManager = new EventManager(); // Ok, we can do better.

        EnabledPlugins = [];
        LoadedPlugins = [];
    }

    public void LoadPlugin(IPlugin plugin)
    {
        if (LoadedPlugins.Contains(plugin))
        {
            // ignore if loaded.
            return;
        }

        try
        {
            plugin.OnLoad(this);

            // After load, enable the plugin.
            EnablePlugin(plugin);

            LoadedPlugins.Add(plugin);
        }
        catch (Exception ex)
        {
            // TODO: handle this error
            Console.WriteLine(string.Format("Failed to load plugin ({0}): {1}", plugin.Name, ex.Message));
        }
    }

    public void UnloadPlugin(IPlugin plugin)
    {
        if (!LoadedPlugins.Contains(plugin))
        {
            // ignore if not loaded.
            return;
        }

        // Before unload, disable the plugin.
        DisablePlugin(plugin);

        try
        {
            plugin.OnUnload(this);

            LoadedPlugins.Remove(plugin);
        }
        catch (Exception ex)
        {
            // TODO: handle this error
            Console.WriteLine(string.Format("Failed to unload plugin ({0}): {1}", plugin.Name, ex.Message));
        }
    }

    public void UnloadPlugins()
    {
        foreach (var plugin in EnabledPlugins)
        {
            UnloadPlugin(plugin);
        }
    }

    public void EnablePlugin(IPlugin plugin)
    {
        if (!LoadedPlugins.Contains(plugin))
        {
            // ignore if not loaded.
            return;
        }

        if (EnabledPlugins.Contains(plugin))
        {
            // ignore if enabled.
            return;
        }

        EventManager.Send<PluginEnableEvent>(plugin);

        EnabledPlugins.Add(plugin);
    }

    public void DisablePlugin(IPlugin plugin)
    {
        if (!LoadedPlugins.Contains(plugin))
        {
            // ignore if not loaded.
            return;
        }

        if (!EnabledPlugins.Contains(plugin))
        {
            // ignore if not enabled.
            return;
        }

        EventManager.Send<PluginDisableEvent>(plugin);

        EnabledPlugins.Remove(plugin);
    }

    public void DisablePlugins()
    {
        foreach (var plugin in EnabledPlugins)
        {
            DisablePlugin(plugin);
        }
    }

    public IEnumerable<IPlugin> GetPlugins()
    {
        return EnabledPlugins;
    }

    public IPlugin? GetPlugin(string name)
    {
        return EnabledPlugins.FirstOrDefault(p => p.Name == name);
    }

    public bool Contains(IPlugin plugin)
    {
        return EnabledPlugins.Contains(plugin);
    }

    public void RegisterListener(IEventListener listener)
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

            EventManager.Register(e, methodAttribute.Priority, method);
        }
    }

    public T SendEvent<T>(params object[] args)
        where T : IEvent
    {
        return EventManager.Send<T>(args);
    }
}
