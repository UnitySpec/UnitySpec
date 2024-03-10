using BoDi;
using System;

namespace UnityFlow.Infrastructure
{
    public class TestObjectResolver : ITestObjectResolver
    {
        public object ResolveBindingInstance(Type bindingType, IObjectContainer container)
        {
            return container.Resolve(bindingType);
        }
    }
}
