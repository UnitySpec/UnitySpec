using UnityFlow.Bindings;

namespace UnityFlow.Infrastructure
{
    public interface IObsoleteStepHandler
    {
        void Handle(BindingMatch bindingMatch);
    }
}