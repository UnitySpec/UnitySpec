using UnitySpec.Bindings;

namespace UnitySpec.Infrastructure
{
    public interface IObsoleteStepHandler
    {
        void Handle(BindingMatch bindingMatch);
    }
}