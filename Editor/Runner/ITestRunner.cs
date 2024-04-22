using System.Collections;

namespace UnitySpec
{
    public interface ITestRunner
    {
        int ThreadId { get; }
        FeatureContext FeatureContext { get; }
        ScenarioContext ScenarioContext { get; }

        void InitializeTestRunner(int threadId);

        IEnumerator OnTestRunStart();
        IEnumerator OnTestRunEnd();

        IEnumerator OnFeatureStart(FeatureInfo featureInfo);
        IEnumerator OnFeatureEnd();

        void OnScenarioInitialize(ScenarioInfo scenarioInfo);
        void OnScenarioStart();

        void CollectScenarioErrors();
        void OnScenarioEnd();

        void SkipScenario();

        object Given(string text, string multilineTextArg, Table tableArg, string keyword = null);
        object When(string text, string multilineTextArg, Table tableArg, string keyword = null);
        object Then(string text, string multilineTextArg, Table tableArg, string keyword = null);
        object And(string text, string multilineTextArg, Table tableArg, string keyword = null);
        object But(string text, string multilineTextArg, Table tableArg, string keyword = null);

        void Pending();
    }

    public static class TestRunnerDefaultArguments
    {
        public static void Given(this ITestRunner testRunner, string text)
        {
            testRunner.Given(text, null, null, null);
        }

        public static void When(this ITestRunner testRunner, string text)
        {
            testRunner.When(text, null, null, null);
        }

        public static void Then(this ITestRunner testRunner, string text)
        {
            testRunner.Then(text, null, null, null);
        }

        public static void And(this ITestRunner testRunner, string text)
        {
            testRunner.And(text, null, null, null);
        }

        public static void But(this ITestRunner testRunner, string text)
        {
            testRunner.But(text, null, null, null);
        }
    }
}
