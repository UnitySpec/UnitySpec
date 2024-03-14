using BoDi;

namespace UnitySpec.Infrastructure
{
    public interface IDefaultDependencyProvider
    {
        void RegisterGlobalContainerDefaults(ObjectContainer container);
        void RegisterTestThreadContainerDefaults(ObjectContainer testThreadContainer);
        void RegisterScenarioContainerDefaults(ObjectContainer scenarioContainer);
    }
}