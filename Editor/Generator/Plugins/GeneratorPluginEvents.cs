using BoDi;
using System;
using UnitySpec.Generator.Configuration;

namespace UnitySpec.Generator.Plugins
{
    public class GeneratorPluginEvents
    {
        public event EventHandler<RegisterDependenciesEventArgs> RegisterDependencies;
        public event EventHandler<CustomizeDependenciesEventArgs> CustomizeDependencies;
        public event EventHandler<ConfigurationDefaultsEventArgs> ConfigurationDefaults;

        public void RaiseRegisterDependencies(ObjectContainer objectContainer)
        {
            RegisterDependencies?.Invoke(this, new RegisterDependenciesEventArgs(objectContainer));
        }

        public void RaiseConfigurationDefaults(SpecFlowProjectConfiguration specFlowProjectConfiguration)
        {
            ConfigurationDefaults?.Invoke(this, new ConfigurationDefaultsEventArgs(specFlowProjectConfiguration));
        }

        public void RaiseCustomizeDependencies(ObjectContainer container, SpecFlowProjectConfiguration specFlowProjectConfiguration)
        {
            CustomizeDependencies?.Invoke(this, new CustomizeDependenciesEventArgs(container, specFlowProjectConfiguration));
        }
    }
}
