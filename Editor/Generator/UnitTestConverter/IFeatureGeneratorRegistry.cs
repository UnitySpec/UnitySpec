using UnitySpec.General.Parser;

namespace UnitySpec.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorRegistry
    {
        IFeatureGenerator CreateGenerator(SpecFlowDocument document);

    }
}