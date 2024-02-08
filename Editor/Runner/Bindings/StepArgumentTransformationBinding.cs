using System.Text.RegularExpressions;
using UnityFlow.Bindings.Reflection;

namespace UnityFlow.Bindings
{
    public class StepArgumentTransformationBinding : MethodBinding, IStepArgumentTransformationBinding
    {
        public Regex Regex { get; private set; }

        public StepArgumentTransformationBinding(Regex regex, IBindingMethod bindingMethod)
            : base(bindingMethod)
        {
            Regex = regex;
        }

        public StepArgumentTransformationBinding(string regexString, IBindingMethod bindingMethod)
            : this(RegexFactory.Create(regexString), bindingMethod)
        {
        }
    }
}