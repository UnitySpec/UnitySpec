using System;
using UnitySpec.Infrastructure;
using UnitySpec.Tracing;

namespace UnitySpec.Bindings
{
    public interface IBindingInvoker
    {
        object InvokeBinding(IBinding binding, IContextManager contextManager, object[] arguments, ITestTracer testTracer, out TimeSpan duration);
    }
}