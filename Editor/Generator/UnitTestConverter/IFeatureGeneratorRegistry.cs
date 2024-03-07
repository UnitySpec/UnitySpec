using UnityFlow.General.Parser;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorRegistry
    {
        IFeatureGenerator CreateGenerator(SpecFlowDocument document);

    }
}