using System.Globalization;
using UnityFlow.Bindings;

namespace UnityFlow.BindingSkeletons
{
    public interface IStepDefinitionSkeletonProvider
    {
        string GetBindingClassSkeleton(ProgrammingLanguage language, StepInstance[] stepInstances, string namespaceName, string className, StepDefinitionSkeletonStyle style, CultureInfo bindingCulture);
        string GetStepDefinitionSkeleton(ProgrammingLanguage language, StepInstance stepInstance, StepDefinitionSkeletonStyle style, CultureInfo bindingCulture);
    }
}