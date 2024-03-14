using UnitySpec.General.Plugins;

namespace UnitySpec.Generator.Plugins
{
    public interface IGeneratorPluginLoader
    {
        IGeneratorPlugin LoadPlugin(PluginDescriptor pluginDescriptor);
    }
}
