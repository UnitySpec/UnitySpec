namespace UnitySpec
{
    public enum ScenarioExecutionStatus
    {
        OK,
        StepDefinitionPending,
        UndefinedStep,
        BindingError,
        TestError,
        Skipped
    }
}