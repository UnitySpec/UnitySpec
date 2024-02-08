namespace UnityFlow.Configuration
{
    public interface IRuntimeConfigurationProvider
    {
        SpecFlowConfiguration LoadConfiguration(SpecFlowConfiguration specFlowConfiguration);
    }
}
