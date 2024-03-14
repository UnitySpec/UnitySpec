using UnitySpec.General.Tracing;

namespace UnitySpec.Tracing
{
    public class NullListener : ITraceListener, IThreadSafeTraceListener
    {
        public void WriteTestOutput(string message)
        {
            //nop;
        }

        public void WriteToolOutput(string message)
        {
            //nop;
        }
    }
}