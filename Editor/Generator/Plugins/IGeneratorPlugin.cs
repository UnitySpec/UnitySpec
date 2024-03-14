using UnitySpec.General.UnitTestProvider;

namespace UnitySpec.Generator.Plugins
{
    public interface IGeneratorPlugin
    {
        void Initialize(GeneratorPluginEvents generatorPluginEvents, GeneratorPluginParameters generatorPluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration);
    }
}