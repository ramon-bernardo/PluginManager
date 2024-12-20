using System.Reflection;

namespace PluginManager.Plugins;

public static class PluginLoader
{
    public static IList<IPlugin> LoadPlugins(FileInfo file)
    {
        var assembly = Assembly.LoadFrom(file.FullName);
        if (assembly == null)
        {
            return [];
        }

        return LoadPlugins(assembly);
    }

    public static IList<IPlugin> LoadPlugins(Assembly assembly)
    {
        var plugins = assembly.GetTypes()
                              .Where(type => !type.IsInterface && !type.IsAbstract && typeof(IPlugin).IsAssignableFrom(type))
                              .Select(type => (IPlugin?)Activator.CreateInstance(type))
                              .OfType<IPlugin>();

        return plugins.ToArray();
    }
}
