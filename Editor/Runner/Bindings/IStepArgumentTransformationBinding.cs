using System.Text.RegularExpressions;

namespace UnityFlow.Bindings
{
    public interface IStepArgumentTransformationBinding : IBinding
    {
        Regex Regex { get; }
    }
}