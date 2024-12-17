namespace PluginManager.Events;

public interface IEvent { }

public interface ICancellableEvent
{
    bool Cancelled { get; set; }
}
