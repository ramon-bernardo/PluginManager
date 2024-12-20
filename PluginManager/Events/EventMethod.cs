using System.Reflection;
using PluginManager.Plugins;

namespace PluginManager.Events;

internal class EventMethod(IPlugin plugin, MethodInfo method)
{
    internal readonly IPlugin Plugin = plugin;
    internal readonly MethodInfo Method = method;
}
