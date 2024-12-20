using PluginManager.Events;
using PluginManager.Plugins;

namespace PluginManager.Examples;

internal class ChatPriorityExample
{
    public static void ChatPriorityExampleMain(string[] args)
    {
        var manager = new Plugins.PluginManager();
        manager.LoadPlugin(new ExamplePlugin());

        var messages = new List<string>()
        {
            "Hello, World!",
            "!online",
            "!!!"
        };

        foreach (var message in messages)
        {
            var e = manager.SendEvent(new ChatEvent(message));
            if (!e.Cancelled)
            {
                Console.WriteLine(e.Message);
            }
        }
    }

    private sealed class ExamplePlugin : IPlugin
    {
        public string Name => "ExamplePlugin";
        public Version Version => new(1, 0, 0);

        public void OnLoad(Plugins.PluginManager manager)
        {
            var listener = new ExampleListener(manager);
            manager.RegisterListener(this, listener);
        }
    }

    private sealed class ExampleListener : IEventListener
    {
        private readonly Plugins.PluginManager PluginManager;

        public ExampleListener(Plugins.PluginManager manager)
        {
            PluginManager = manager;
        }

        [Event]
        public void OnSay(ChatEvent e)
        {
            if (e.Cancelled)
            {
                // ignore if cancelled
                return;
            }

            Console.WriteLine(string.Format("Listening chat events, message: {0}", e.Message));
        }

        [Event(Priority = EventPriority.High)]
        public void OnSayCommand(ChatEvent e)
        {
            if (e.Cancelled)
            {
                // ignore if cancelled
                return;
            }

            if (e.Message.Length < 1)
            {
                return;
            }

            const char commandPrefix = '!';
            if (!e.Message.StartsWith(commandPrefix))
            {
                return;
            }

            var parts = e.Message[1..].Split(' ', StringSplitOptions.RemoveEmptyEntries);

            var command = parts[0];
            switch (command)
            {
                case "online":
                    e.Cancelled = true;

                    PluginManager.SendEvent(new OnlineCommand());
                    break;
                default:
                    break;
            }
        }

        [Event]
        public void OnReceiveOnlineCommand(OnlineCommand e)
        {
            Console.WriteLine("Received online command.");
        }
    }

    private sealed class ChatEvent : IEvent, ICancellableEvent
    {
        public bool Cancelled { get; set; }

        public string Message { get; }

        public ChatEvent(string message)
        {
            Message = message;
        }
    }

    private sealed class OnlineCommand : IEvent { }
}
