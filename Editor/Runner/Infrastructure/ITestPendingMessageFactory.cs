namespace UnitySpec.Infrastructure
{
    public interface ITestPendingMessageFactory
    {
        string BuildFromScenarioContext(ScenarioContext scenarioContext);
    }
}
