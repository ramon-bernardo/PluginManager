using PluginManager.Plugins;

namespace PluginManager.Events.Plugin;

public sealed class PluginEnableEvent(IPlugin plugin) : PluginEvent(plugin) { }
