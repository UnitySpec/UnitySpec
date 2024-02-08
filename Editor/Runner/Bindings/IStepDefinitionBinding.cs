using System.Text.RegularExpressions;

namespace UnityFlow.Bindings
{
    public interface IStepDefinitionBinding : IScopedBinding, IBinding
    {
        StepDefinitionType StepDefinitionType { get; }
        Regex Regex { get; }
    }
}