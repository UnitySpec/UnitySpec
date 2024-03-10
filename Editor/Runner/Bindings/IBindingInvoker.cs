using System;
using UnityFlow.Infrastructure;
using UnityFlow.Tracing;

namespace UnityFlow.Bindings
{
    public interface IBindingInvoker
    {
        object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration);
    }
}