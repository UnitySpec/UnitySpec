using System;
using System.Globalization;
using UnityFlow.Configuration;
using UnityFlow.ErrorHandling;
using UnityFlow.BindingSkeletons;

namespace UnityFlow.Infrastructure
{
    public class TestUndefinedMessageFactory : ITestUndefinedMessageFactory
    {
        private readonly IStepDefinitionSkeletonProvider _stepDefinitionSkeletonProvider;
        private readonly IErrorProvider _errorProvider;
        private readonly SpecFlowConfiguration _specFlowConfiguration;

        public TestUndefinedMessageFactory(IStepDefinitionSkeletonProvider stepDefinitionSkeletonProvider, IErrorProvider errorProvider, SpecFlowConfiguration specFlowConfiguration)
        {
            _stepDefinitionSkeletonProvider = stepDefinitionSkeletonProvider;
            _errorProvider = errorProvider;
            _specFlowConfiguration = specFlowConfiguration;
        }

        public string BuildFromContext(ScenarioContext scenarioContext, FeatureContext featureContext)
        {
            string skeleton = _stepDefinitionSkeletonProvider.GetBindingClassSkeleton(
                featureContext.FeatureInfo.GenerationTargetLanguage,
                scenarioContext.MissingSteps.ToArray(),
                "MyNamespace",
                "StepDefinitions",
                _specFlowConfiguration.StepDefinitionSkeletonStyle,
                featureContext.BindingCulture ?? CultureInfo.CurrentCulture);

            return $"{_errorProvider.GetMissingStepDefinitionError().Message}{Environment.NewLine}{skeleton}";
        }
    }
}
