using System;

namespace UnitySpec.Generator.UnitTestProvider
{
    [Flags]
    public enum UnitTestGeneratorTraits
    {
        None = 0,
        RowTests = 1,
        ParallelExecution = 2
    }
}