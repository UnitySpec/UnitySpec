using UnitySpec.Bindings;

namespace UnitySpec.Infrastructure
{
    public interface ITestExecutionEngine
    {
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }

        void OnTestRunStart();
        void OnTestRunEnd();

        void OnFeatureStart(FeatureInfo featureInfo);
        void OnFeatureEnd();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo);
        void OnScenarioStart();
        void OnAfterLastStep();
        void OnScenarioEnd();

        void OnScenarioSkipped();

        object Step(StepDefinitionKeyword stepDefinitionKeyword, string keyword, string text, string multilineTextArg, Table tableArg);

        void Pending();
    }
}
