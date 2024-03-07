using BoDi;
using UnityFlow.Generator.UnitTestProvider;

namespace UnityFlow.Generator
{
    partial class DefaultDependencyProvider
    {
        partial void RegisterUnitTestGeneratorProviders(ObjectContainer container)
        {
            container.RegisterTypeAs<UTFTestGeneratorProvider, IUnitTestGeneratorProvider>("utf");
        }
    }
}
