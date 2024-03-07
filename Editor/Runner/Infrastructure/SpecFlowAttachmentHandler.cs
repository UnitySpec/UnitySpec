
using UnityFlow.General.Tracing;
using UnityFlow.Tracing;

namespace UnityFlow.Infrastructure
{
    public class SpecFlowAttachmentHandler : ISpecFlowAttachmentHandler
    {
        private readonly ITraceListener _traceListener;

        protected SpecFlowAttachmentHandler(ITraceListener traceListener)
        {
            _traceListener = traceListener;
        }

        public virtual void AddAttachment(string filePath)
        {
            _traceListener.WriteToolOutput($"Attachment '{filePath}' added (not forwarded to the test runner).");
        }
    }
}
