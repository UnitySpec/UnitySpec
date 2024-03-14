using System;
using System.Runtime.Serialization;

// the exceptions are part of the public API, keep them in TechTalk.SpecFlow namespace
namespace UnitySpec.ErrorHandling
{
    [Serializable]
    public class MissingStepDefinitionException : SpecFlowException
    {
        public MissingStepDefinitionException()
            : base("No matching step definition found for one or more steps. Use the following code to create it:")
        {
        }

        protected MissingStepDefinitionException(
            SerializationInfo info,
            StreamingContext context)
            : base(info, context)
        {
        }
    }
}