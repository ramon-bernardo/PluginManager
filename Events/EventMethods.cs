using System.Reflection;
using PluginManager.Plugins;

namespace PluginManager.Events;

internal class EventMethods(IPlugin plugin) : Dictionary<EventPriority, IList<MethodInfo>>
{
    internal readonly IPlugin Plugin = plugin;
}
