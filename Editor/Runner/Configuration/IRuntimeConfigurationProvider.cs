using UnitySpec.General.Configuration;

namespace UnitySpec.Runner.Configuration
{
    public interface IRuntimeConfigurationProvider
    {
        SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration);
    }
}