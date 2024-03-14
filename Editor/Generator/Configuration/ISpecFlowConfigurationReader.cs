using UnitySpec.General.GeneratorInterfaces;

namespace UnitySpec.Generator.Configuration
{
    public interface ISpecFlowConfigurationReader
    {
        SpecFlowConfigurationHolder ReadConfiguration();
    }
}