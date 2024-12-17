namespace PluginManager.Plugins;

public interface IPlugin
{
    string Name { get; }

    Version Version { get; }

    void OnLoad(PluginManager manager) { }
    void OnUnload(PluginManager manager) { }
}
