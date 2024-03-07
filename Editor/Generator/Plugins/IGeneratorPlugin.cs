using UnityFlow.General.UnitTestProvider;

namespace UnityFlow.Generator.Plugins
{
    public interface IGeneratorPlugin
    {
        void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration);
    }
}