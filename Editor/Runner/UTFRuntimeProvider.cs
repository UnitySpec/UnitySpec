using UnityEngine.Assertions;
using UnitySpec.Runner.UnitTestProvider;

namespace UnitySpec
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