using UnityFlow.UnitTestProvider;

namespace UnityFlow.Plugins
{
    public interface IRuntimePlugin
    {
        void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters, UnitTestProviderConfiguration unitTestProviderConfiguration);
    }
}
