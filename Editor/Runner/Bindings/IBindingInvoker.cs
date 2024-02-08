using UnityFlow.Infrastructure;
using UnityFlow.Tracing;
using System;

namespace UnityFlow.Bindings
{
    public interface IBindingInvoker
    {
        object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration);
    }
}