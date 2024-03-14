using System.Text.RegularExpressions;

namespace UnitySpec.Bindings
{
    public interface IStepDefinitionBinding : IScopedBinding, IBinding
    {
        StepDefinitionType StepDefinitionType { get; }
        Regex Regex { get; }
    }
}