using UnityEngine.Assertions;
using UnityFlow.Runner.UnitTestProvider;

namespace UnityFlow
{
    public class UTFRuntimeProvider : IUnitTestRuntimeProvider
    {
        public void TestPending(string message)
        {
            TestInconclusive(message);
        }

        public void TestInconclusive(string message)
        {
            Assert.IsTrue(false, message);
        }

        public void TestIgnore(string message)
        {
            TestInconclusive(message);
        }

        public bool DelayedFixtureTearDown => true;
    }
}