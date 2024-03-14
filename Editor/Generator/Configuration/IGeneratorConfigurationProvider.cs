using UnitySpec.General.Configuration;
using UnitySpec.General.GeneratorInterfaces;

namespace UnitySpec.Generator.Configuration
{
    public interface IGeneratorConfigurationProvider
    {
        SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration, SpecFlowConfigurationHolder specFlowConfigurationHolder);
        SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration);
    }

    public static class GeneratorConfigurationProviderExtensions
    {
        public static SpecFlowProjectConfiguration LoadConfiguration(this IGeneratorConfigurationProvider configurationProvider, SpecFlowConfigurationHolder configurationHolder)
        {
            SpecFlowProjectConfiguration configuration = new SpecFlowProjectConfiguration();
            configuration.SpecFlowConfiguration = configurationProvider.LoadConfiguration(configuration.SpecFlowConfiguration, configurationHolder);

            return configuration;
        }
    }
}
