using UnityFlow.General.Plugins;

namespace UnityFlow.Generator.Plugins
{
    public interface IGeneratorPluginLoader
    {
        IGeneratorPlugin LoadPlugin(PluginDescriptor pluginDescriptor);
    }
}
