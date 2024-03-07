using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityFlow.Generator.UnitTestConverter
{
    public class IgnoreDecorator : ITestClassTagDecorator, ITestMethodTagDecorator
    {
        private const string IGNORE_TAG = "ignore";
        private readonly ITagFilterMatcher tagFilterMatcher;

        public int Priority
        {
            get { return PriorityValues.Low; }
        }

        public bool RemoveProcessedTags
        {
            get { return true; }
        }

        public bool ApplyOtherDecoratorsForProcessedTags
        {
            get { return false; }
        }

        public IgnoreDecorator(ITagFilterMatcher tagFilterMatcher)
        {
            this.tagFilterMatcher = tagFilterMatcher;
        }

        private bool CanDecorateFrom(string tagName)
        {
            return tagFilterMatcher.Match(IGNORE_TAG, tagName);
        }

        public bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            return CanDecorateFrom(tagName);
        }

        public MethodDeclarationSyntax DecorateFrom(string tagName, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            return generationContext.UnitTestGeneratorProvider.SetTestMethodIgnore(testMethod);
        }

        public bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext)
        {
            return CanDecorateFrom(tagName);
        }

        public void DecorateFrom(string tagName, TestClassGenerationContext generationContext)
        {
            generationContext.UnitTestGeneratorProvider.SetTestClassIgnore(generationContext);
        }
    }
}
