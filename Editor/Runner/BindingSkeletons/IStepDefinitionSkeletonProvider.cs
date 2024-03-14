using System.Globalization;
using UnitySpec.Bindings;
using UnitySpec.General.BindingSkeletons;

namespace UnitySpec.BindingSkeletons
{
    public interface IStepDefinitionSkeletonProvider
    {
        string GetBindingClassSkeleton(ProgrammingLanguage language, StepInstance[] stepInstances, string namespaceName, string className, StepDefinitionSkeletonStyle style, CultureInfo bindingCulture);
        string GetStepDefinitionSkeleton(ProgrammingLanguage language, StepInstance stepInstance, StepDefinitionSkeletonStyle style, CultureInfo bindingCulture);
    }
}