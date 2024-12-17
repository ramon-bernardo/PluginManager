using PluginManager.Plugins;

namespace PluginManager.Events.Plugin;

public sealed class PluginDisableEvent(IPlugin plugin) : PluginEvent(plugin) { }
