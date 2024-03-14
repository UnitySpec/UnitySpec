using System.Text.RegularExpressions;

namespace UnitySpec.Bindings
{
    public interface IStepArgumentTransformationBinding : IBinding
    {
        Regex Regex { get; }
    }
}