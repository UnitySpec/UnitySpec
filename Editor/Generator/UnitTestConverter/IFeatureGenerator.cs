using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnitySpec.General.Parser;

namespace UnitySpec.Generator.UnitTestConverter
{
    public interface IFeatureGenerator
    {
        NamespaceDeclarationSyntax GenerateUnitTestFixture(SpecFlowDocument document, string testClassName, string targetNamespace);
    }
}