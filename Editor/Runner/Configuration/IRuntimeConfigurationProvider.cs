using UnityFlow.General.Configuration;

namespace UnityFlow.Runner.Configuration
{
    public interface IRuntimeConfigurationProvider
    {
        SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration);
    }
}