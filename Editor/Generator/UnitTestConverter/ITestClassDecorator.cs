using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface ITestXDecorator
    {
        int Priority { get; }
    }
    public interface ITestClassTagDecorator 
    {
        int Priority { get; }
        bool RemoveProcessedTags { get; }
        bool ApplyOtherDecoratorsForProcessedTags { get; }

        bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext);
        void DecorateFrom(string tagName, TestClassGenerationContext generationContext);
    }

    public interface ITestMethodTagDecorator
    {
        int Priority { get; }
        bool RemoveProcessedTags { get; }
        bool ApplyOtherDecoratorsForProcessedTags { get; }

        bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod);
        MethodDeclarationSyntax DecorateFrom(string tagName, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod);
    }

    public interface ITestClassDecorator : ITestXDecorator
    {
        bool CanDecorateFrom(TestClassGenerationContext generationContext);
        void DecorateFrom(TestClassGenerationContext generationContext);
    }

    public interface ITestMethodDecorator : ITestXDecorator
    {
        bool CanDecorateFrom(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod);
        MethodDeclarationSyntax DecorateFrom(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod);
    }
}
