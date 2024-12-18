# Plugin Manager

A lightweight and extensible plugin management library inspired by the [Bukkit](https://bukkit.org/) plugin system, it offers a structured approach to loading, enabling, disabling, and unloading plugins in your apps.

Beyond managing the plugin lifecycle, the library features a robust event system that allows seamless communication between plugins. You can define `custom events`, register `listeners` and control event execution using `priorities`, with the option to mark as `cancellable`.

## Usage

### Creating a plugin

To create a plugin, implement the `IPlugin` interface. This requires specifying the plugin name, version and implementing the `OnLoad` and `OnUnload` as optional methods.

```csharp
public class ExamplePlugin : IPlugin
{
    public string Name => "ExamplePlugin";

    public Version Version => new Version(1, 0, 0);

    public void OnLoad(PluginManager manager)
    {
        Console.WriteLine(string.Format("{0} v{1} has been loaded.", Name, Version));
    }

    public void OnUnload(PluginManager manager)
    {
        Console.WriteLine(string.Format("{0} v{1} has been unloaded.", Name, Version));
    }
}
```

### Managing plugins

Use the `PluginManager` class to dynamically load and manage your plugins. Plugins can either be instantiated manually or loaded directly from assemblies or files using the provided loader utility.

#### Manually Loading Plugins

```csharp
var manager = new PluginManager();

var plugin = new ExamplePlugin();
manager.LoadPlugin(plugin);
manager.UnloadPlugin(plugin);
```

#### Dynamically Loading Plugins

The `PluginManager` includes utilities to discover and load plugins directly from `.dll` files or assembly objects.

##### From a File

```csharp
var manager = new PluginManager();

var fileInfo = new FileInfo("example_plugin.dll");
var plugins = PluginLoader.LoadPlugins(fileInfo);

foreach (var plugin in plugins)
{
    manager.LoadPlugin(plugin);
}
```

##### From an Assembly

```csharp
var manager = new PluginManager();

var assembly = Assembly.LoadFrom("example_plugin.dll");
var plugins = PluginLoader.LoadPlugins(assembly);

foreach (var plugin in plugins)
{
    manager.LoadPlugin(plugin);
}
```

## Event Handling

Plugins can interact with custom events through the event system. Events are defined by implementing the `IEvent` interface. For cancellable events, use `ICancellableEvent` interface.

Use the `Event` attribute to handle events and the `Priority` (`Low`, `Normal`, and `High`) property to define the execution priority.

#### Defining an Event

```csharp
public class ExampleEvent : IEvent
{
    public string Message { get; }

    public ExampleEvent(string message)
    {
        Message = message;
    }
}
```

#### Creating a High-Priority Event Listener

```csharp
public class ExampleListener : IEventListener
{
    [Event(Priority = EventPriority.High)]
    public void OnExampleEvent(ExampleEvent e)
    {
        Console.WriteLine(string.Format("Received event: {0}", e.Message));
    }
}
```

#### Registering the Listener in a Plugin

```csharp
public class ExamplePlugin : IPlugin
{
    // ...

    public void OnLoad(PluginManager manager)
    {
        var listener = new ExampleListener();
        manager.RegisterListener(this, listener);
    }
}
```

#### Sending Events

Use the `SendEvent` method to dispatch events:

```csharp
var e = manager.SendEvent<ExampleEvent>("Hello, Plugin!");
```