using UnitySpec.General.Parser;

namespace UnitySpec.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorProvider
    {
        int Priority { get; }
        bool CanGenerate(SpecFlowDocument document);
        IFeatureGenerator CreateGenerator(SpecFlowDocument document);
    }
}