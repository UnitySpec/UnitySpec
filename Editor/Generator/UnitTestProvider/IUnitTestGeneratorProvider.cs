using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace UnityFlow.Generator.UnitTestProvider
{
    public interface IUnitTestGeneratorProvider
    {
        UnitTestGeneratorTraits GetTraits();

        void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription);
        void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories);
        void SetTestClassIgnore(TestClassGenerationContext generationContext);
        void FinalizeTestClass(TestClassGenerationContext generationContext);
        void SetTestClassNonParallelizable(TestClassGenerationContext generationContext);

        void SetTestClassInitializeMethod(TestClassGenerationContext generationContext);
        void SetTestClassCleanupMethod(TestClassGenerationContext generationContext);

        void SetTestInitializeMethod(TestClassGenerationContext generationContext);
        void SetTestCleanupMethod(TestClassGenerationContext generationContext);

        void SetTestMethod(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, string friendlyTestName);
        void SetTestMethodCategories(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<string> scenarioCategories);
        void SetTestMethodIgnore(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod);

        void SetRowTest(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, string scenarioTitle);
        void SetRow(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored);
        void SetTestMethodAsRow(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments);
    }
}
