using System.Reflection;

namespace UnityFlow.Infrastructure
{
    public interface ITestAssemblyProvider
    {
        Assembly TestAssembly { get; }

        public void RegisterTestAssembly(Assembly testAssembly);
    }
}
