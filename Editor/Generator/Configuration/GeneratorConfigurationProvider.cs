using UnitySpec.General.Configuration;
using UnitySpec.General.GeneratorInterfaces;

namespace UnitySpec.Generator.Configuration
{
    public class GeneratorConfigurationProvider : IGeneratorConfigurationProvider
    {
        private readonly IConfigurationLoader _configurationLoader;

        public GeneratorConfigurationProvider(IConfigurationLoader configurationLoader)
        {
            _configurationLoader = configurationLoader;
        }

        public virtual SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration, SpecFlowConfigurationHolder specFlowConfigurationHolder)
        {
            return _configurationLoader.Load(specFlowConfiguration, specFlowConfigurationHolder);
        }

        public SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration)
        {
            return _configurationLoader.Load(specFlowConfiguration);
        }

        internal virtual void UpdateConfiguration(SpecFlowProjectConfiguration configuration, ConfigurationSectionHandler specFlowConfigSection)
        {
            configuration.SpecFlowConfiguration = _configurationLoader.Update(configuration.SpecFlowConfiguration, specFlowConfigSection);
        }

    }
}