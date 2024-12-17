using PluginManager.Plugins;

namespace PluginManager.Events.Plugin;

public abstract class PluginEvent(IPlugin plugin) : IEvent
{
    public IPlugin Plugin { get; } = plugin;
}
