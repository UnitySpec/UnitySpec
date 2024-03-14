using System.Reflection;

namespace UnitySpec.Infrastructure
{
    public interface ITestRunnerFactory
    {
        TestRunner Create(Assembly testAssembly);
    }
}
