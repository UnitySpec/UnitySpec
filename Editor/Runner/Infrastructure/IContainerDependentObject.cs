using BoDi;

namespace UnityFlow.Infrastructure
{
    public interface IContainerDependentObject
    {
        void SetObjectContainer(IObjectContainer container);
    }
}
