using System.Reflection;

namespace UnitySpec.Bindings.Discovery
{
    public interface IRuntimeBindingRegistryBuilder
    {
        void BuildBindingsFromAssembly(Assembly assembly);
        void BuildingCompleted();
    }
}