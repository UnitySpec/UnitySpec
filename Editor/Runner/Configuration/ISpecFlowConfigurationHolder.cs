namespace UnityFlow.Configuration
{
    public interface ISpecFlowConfigurationHolder
    {
        ConfigSource ConfigSource { get; }
        string Content { get; }
        bool HasConfiguration { get; }
    }
}