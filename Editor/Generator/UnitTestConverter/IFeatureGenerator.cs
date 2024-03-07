using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityFlow.General.Parser;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface IFeatureGenerator
    {
        NamespaceDeclarationSyntax GenerateUnitTestFixture(SpecFlowDocument document, string testClassName, string targetNamespace);
    }
}