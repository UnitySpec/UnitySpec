using UnityFlow.General.Parser;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorProvider
    {
        int Priority { get; }
        bool CanGenerate(SpecFlowDocument document);
        IFeatureGenerator CreateGenerator(SpecFlowDocument document);
    }
}