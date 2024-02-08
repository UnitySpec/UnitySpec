using System;

namespace UnityFlow.Tracing
{
    public class DefaultListener : ITraceListener
    {
        public void WriteTestOutput(string message)
        {
            Console.WriteLine(message);
            UnityEngine.Debug.Log(message);
        }

        public void WriteToolOutput(string message)
        {
            Console.WriteLine("-> " + message);
            UnityEngine.Debug.Log("-> " + message);
        }
    }
}