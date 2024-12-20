﻿using System.Reflection;
using PluginManager.Plugins;

namespace PluginManager.Events;

internal class EventMethod(IPlugin plugin, IEventListener listener, MethodInfo method)
{
    internal readonly IPlugin Plugin = plugin;
    internal readonly IEventListener Listener = listener;
    internal readonly MethodInfo Method = method;
}
