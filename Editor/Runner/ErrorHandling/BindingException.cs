using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in TechTalk.SpecFlow namespace
namespace UnityFlow.ErrorHandling
{
    [Serializable]
    public class BindingException : SpecFlowException
    {
        public BindingException()
        {
        }

        public BindingException(string message) : base(message)
        {
        }

        public BindingException(string message, Exception inner) : base(message, inner)
        {
        }

        protected BindingException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}