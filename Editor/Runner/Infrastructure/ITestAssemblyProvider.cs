using System.Reflection;

namespace UnitySpec.Infrastructure
{
    public interface ITestAssemblyProvider
    {
        Assembly TestAssembly { get; }

        void RegisterTestAssembly(Assembly testAssembly);
    }
}
