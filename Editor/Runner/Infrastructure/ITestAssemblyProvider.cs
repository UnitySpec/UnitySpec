using System.Reflection;

namespace UnityFlow.Infrastructure
{
    public interface ITestAssemblyProvider
    {
        Assembly TestAssembly { get; }

        void RegisterTestAssembly(Assembly testAssembly);
    }
}
