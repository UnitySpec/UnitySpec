using System.Reflection;

namespace UnitySpec.Infrastructure
{
    public interface IBindingAssemblyLoader
    {
        Assembly Load(string assemblyName);
    }

    public class BindingAssemblyLoader : IBindingAssemblyLoader
    {
        public Assembly Load(string assemblyName)
        {
            return Assembly.Load(assemblyName);
        }
    }
}