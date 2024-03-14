using System.Reflection;

namespace UnitySpec.Infrastructure
{
    public class TestAssemblyProvider : ITestAssemblyProvider
    {
        public Assembly TestAssembly { get; private set; }

        public void RegisterTestAssembly(Assembly testAssembly)
        {
            TestAssembly = testAssembly;
        }

    }
}
