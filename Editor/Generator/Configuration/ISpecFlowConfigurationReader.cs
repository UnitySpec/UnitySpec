using UnityFlow.General.GeneratorInterfaces;

namespace UnityFlow.Generator.Configuration
{
    public interface ISpecFlowConfigurationReader
    {
        SpecFlowConfigurationHolder ReadConfiguration();
    }
}