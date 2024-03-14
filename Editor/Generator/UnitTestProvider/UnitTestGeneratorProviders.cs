using BoDi;
using UnitySpec.Generator.UnitTestProvider;

namespace UnitySpec.Generator
{
    partial class DefaultDependencyProvider
    {
        partial void RegisterUnitTestGeneratorProviders(ObjectContainer container)
        {
            container.RegisterTypeAs<UTFTestGeneratorProvider, IUnitTestGeneratorProvider>("utf");
        }
    }
}
