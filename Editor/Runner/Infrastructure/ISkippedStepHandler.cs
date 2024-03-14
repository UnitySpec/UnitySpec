namespace UnitySpec.Infrastructure
{
    public interface ISkippedStepHandler
    {
        void Handle(ScenarioContext scenarioContext);
    }
}