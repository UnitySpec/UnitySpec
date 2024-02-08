namespace UnityFlow.Infrastructure
{
    public interface ISkippedStepHandler
    {
        void Handle(ScenarioContext scenarioContext);
    }
}