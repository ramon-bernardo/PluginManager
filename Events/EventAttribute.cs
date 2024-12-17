namespace PluginManager.Events;

[AttributeUsage(AttributeTargets.Method)]
public sealed class Event : Attribute
{
    public EventPriority Priority = EventPriority.Normal;
}
