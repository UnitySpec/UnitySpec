using BoDi;

namespace UnitySpec.Infrastructure
{
    public interface IContainerDependentObject
    {
        void SetObjectContainer(IObjectContainer container);
    }
}
