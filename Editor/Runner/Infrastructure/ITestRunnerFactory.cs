using System.Reflection;

namespace UnityFlow.Infrastructure
{
    public interface ITestRunnerFactory
    {
        TestRunner Create(Assembly testAssembly);
    }
}
