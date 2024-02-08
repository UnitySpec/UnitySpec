namespace UnityFlow.Infrastructure
{
    public interface ITestPendingMessageFactory
    {
        string BuildFromScenarioContext(ScenarioContext scenarioContext);
    }
}
