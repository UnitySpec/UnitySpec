using System.Reflection;

namespace UnityFlow.Bindings.Discovery
{
    public interface IRuntimeBindingRegistryBuilder
    {
        void BuildBindingsFromAssembly(Assembly assembly);
        void BuildingCompleted();
    }
}