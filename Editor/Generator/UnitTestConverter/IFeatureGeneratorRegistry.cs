using TechTalk.SpecFlow.Parser;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface IFeatureGeneratorRegistry
    {
        IFeatureGenerator CreateGenerator(SpecFlowDocument document);
    }
}