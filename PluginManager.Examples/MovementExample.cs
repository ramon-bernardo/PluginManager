using PluginManager.Events;
using PluginManager.Plugins;

namespace PluginManager.Examples;

internal class MovementExample
{
    public static void MovementExampleMain(string[] args)
    {
        var manager = new Plugins.PluginManager();
        manager.LoadPlugin(new ExamplePlugin());

        var from = new Position(10, 10);
        var to = new Position(20, 20);

        var e = manager.SendEvent(new MovementEvent(from, to));
        if (e.Cancelled)
        {
            Console.WriteLine("Event was cancelled by observers.");
        }

        if (e.To.X != to.X || e.To.Y != e.To.Y)
        {
            Console.WriteLine("Position was changed by observers.");
        }
    }

    private sealed class ExamplePlugin : IPlugin
    {
        public string Name => "ExamplePlugin";
        public Version Version => new(1, 0, 0);

        public void OnLoad(Plugins.PluginManager manager)
        {
            var listener = new ExampleListener();
            manager.RegisterListener(this, listener);
        }
    }

    private sealed class ExampleListener : IEventListener
    {
        [Event]
        public void OnMove(MovementEvent e)
        {
            if (e.Cancelled)
            {
                // ignore if cancelled by another observer.
                return;
            }

            if (e.From.X != 10 || e.From.Y != 10 || e.To.X != 20 || e.To.Y != 20)
            {
                // if another X/Y position is received, cancel the event.
                e.Cancelled = true;
                return;
            }

            e.To = new Position(50, 50);
        }
    }

    private sealed class MovementEvent : IEvent, ICancellableEvent
    {
        public bool Cancelled { get; set; }

        public Position From { get; }
        public Position To { get; set; }

        public MovementEvent(Position from, Position to)
        {
            From = from;
            To = to;
        }
    }

    private readonly struct Position
    {
        public byte X { get; }
        public byte Y { get; }

        public Position(byte x, byte y)
        {
            X = x;
            Y = y;
        }
    }
}
