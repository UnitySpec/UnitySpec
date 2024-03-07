using System;
using System.Collections.Generic;
using System.Linq;
using BoDi;
using Gherkin.Ast;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using UnityFlow.General.Extensions;

namespace UnityFlow.Generator.UnitTestConverter
{
    public interface IDecoratorRegistry
    {
        void DecorateTestClass(TestClassGenerationContext generationContext, out List<string> unprocessedTags);
        MethodDeclarationSyntax DecorateTestMethod(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<Tag> tags, out List<string> unprocessedTags);
    }

    public class DecoratorRegistry : IDecoratorRegistry
    {
        private readonly List<ITestClassDecorator> testClassDecorators;
        private readonly List<ITestMethodDecorator> testMethodDecorators;

        private readonly List<ITestClassTagDecorator> testClassTagDecorators;
        private readonly List<ITestMethodTagDecorator> testMethodTagDecorators;

        #region Decorator wrappers
        private class TestMethodClassDecoratorWrapper : ITestClassDecorator
        {
            private readonly ITestClassDecorator testClassDecorator;

            public TestMethodClassDecoratorWrapper(ITestClassDecorator testClassDecorator)
            {
                this.testClassDecorator = testClassDecorator;
            }

            public int Priority
            {
                get { return testClassDecorator.Priority; }
            }

            public bool CanDecorateFrom(TestClassGenerationContext generationContext)
            {
                return testClassDecorator.CanDecorateFrom(generationContext);
            }

            public void DecorateFrom(TestClassGenerationContext generationContext)
            {
                 testClassDecorator.DecorateFrom(generationContext);
            }
        }
        private class TestMethodClassTagDecoratorWrapper : ITestClassTagDecorator
        {
            private readonly ITestClassTagDecorator testClassTagDecorator;

            public TestMethodClassTagDecoratorWrapper(ITestClassTagDecorator testClassTagDecorator)
            {
                this.testClassTagDecorator = testClassTagDecorator;
            }

            public int Priority
            {
                get { return testClassTagDecorator.Priority; }
            }

            public bool RemoveProcessedTags
            {
                get { return testClassTagDecorator.RemoveProcessedTags; }
            }

            public bool ApplyOtherDecoratorsForProcessedTags
            {
                get { return testClassTagDecorator.ApplyOtherDecoratorsForProcessedTags; }
            }

            public bool CanDecorateFrom(string tagName, TestClassGenerationContext generationContext)
            {
                return testClassTagDecorator.CanDecorateFrom(tagName, generationContext);
            }

            public void DecorateFrom(string tagName, TestClassGenerationContext generationContext)
            {
                testClassTagDecorator.DecorateFrom(tagName, generationContext);
            }
        }
        #endregion

        public DecoratorRegistry(IObjectContainer objectContainer)
        {
            if (objectContainer == null) throw new ArgumentNullException("objectContainer");

            testClassDecorators = ResolveDecorators<ITestClassDecorator>(objectContainer, d => d);
            testMethodDecorators = ResolveDecorators<ITestMethodDecorator>(objectContainer, d => d);

            testClassTagDecorators = ResolveTagDecorators<ITestClassTagDecorator>(objectContainer, d => d);
            testMethodTagDecorators = ResolveTagDecorators<ITestMethodTagDecorator>(objectContainer, d => d);
        }

        private List<ITestMethodDecorator> ResolveDecorators<TDecorator>(IObjectContainer objectContainer, Func<TDecorator, ITestMethodDecorator> selector)
        {
            return objectContainer.Resolve<IDictionary<string, TDecorator>>().Select(item => selector(item.Value)).OrderBy(d => d.Priority).ToList();
        }
        private List<ITestClassDecorator> ResolveDecorators<TDecorator>(IObjectContainer objectContainer, Func<TDecorator, ITestClassDecorator> selector)
        {
            return objectContainer.Resolve<IDictionary<string, TDecorator>>().Select(item => selector(item.Value)).OrderBy(d => d.Priority).ToList();
        }

        private List<ITestMethodTagDecorator> ResolveTagDecorators<TDecorator>(IObjectContainer objectContainer, Func<TDecorator, ITestMethodTagDecorator> selector)
        {
            return objectContainer.Resolve<IDictionary<string, TDecorator>>().Select(item => selector(item.Value)).OrderBy(d => d.Priority).ToList();
        }

        private List<ITestClassTagDecorator> ResolveTagDecorators<TDecorator>(IObjectContainer objectContainer, Func<TDecorator, ITestClassTagDecorator> selector)
        {
            return objectContainer.Resolve<IDictionary<string, TDecorator>>().Select(item => selector(item.Value)).OrderBy(d => d.Priority).ToList();
        }

        public void DecorateTestClass(TestClassGenerationContext generationContext, out List<string> unprocessedTags)
        {
            Decorate(testClassDecorators, testClassTagDecorators, generationContext, generationContext.Feature.Tags, out unprocessedTags);
        }

        public MethodDeclarationSyntax DecorateTestMethod(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<Tag> tags, out List<string> unprocessedTags)
        {
            return Decorate(testMethodDecorators, testMethodTagDecorators, generationContext, testMethod, tags, out unprocessedTags);
        }

        private MethodDeclarationSyntax Decorate(List<ITestMethodDecorator> decorators, List<ITestMethodTagDecorator> tagDecorators, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<Tag> tags, out List<string> unprocessedTags)
        {
            List<string> featureCategories = new List<string>();

            foreach (var decorator in FindDecorators(decorators, generationContext, testMethod))
            {
                testMethod = decorator.DecorateFrom(generationContext, testMethod);
            }

            if (tags != null)
            {
                foreach (var tagName in tags.Select(t => t.GetNameWithoutAt()))
                {
                    bool removeProcessedTag = false;
                    foreach (var decorator in FindDecorators(tagDecorators, tagName, generationContext, testMethod))
                    {
                        testMethod = decorator.DecorateFrom(tagName, generationContext, testMethod);
                        removeProcessedTag |= decorator.RemoveProcessedTags;
                        if (!decorator.ApplyOtherDecoratorsForProcessedTags)
                            break;
                    }

                    if (!removeProcessedTag)
                        featureCategories.Add(tagName);
                }
            }

            unprocessedTags = featureCategories;
            return testMethod;
        }

        private void Decorate(List<ITestClassDecorator> decorators, List<ITestClassTagDecorator> tagDecorators, TestClassGenerationContext generationContext, IEnumerable<Tag> tags, out List<string> unprocessedTags)
        {
            List<string> featureCategories = new List<string>();

            foreach (var decorator in FindDecorators(decorators, generationContext))
            {
                 decorator.DecorateFrom(generationContext);
            }

            if (tags != null)
            {
                foreach (var tagName in tags.Select(t => t.GetNameWithoutAt()))
                {
                    bool removeProcessedTag = false;
                    foreach (var decorator in FindDecorators(tagDecorators, tagName, generationContext))
                    {
                        decorator.DecorateFrom(tagName, generationContext);
                        removeProcessedTag |= decorator.RemoveProcessedTags;
                        if (!decorator.ApplyOtherDecoratorsForProcessedTags)
                            break;
                    }

                    if (!removeProcessedTag)
                        featureCategories.Add(tagName);
                }
            }

            unprocessedTags = featureCategories;
        }

        private IEnumerable<ITestMethodDecorator> FindDecorators(List<ITestMethodDecorator> decorators, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            return decorators.Where(decorator => decorator.CanDecorateFrom(generationContext, testMethod));
        }

        private IEnumerable<ITestMethodTagDecorator> FindDecorators(List<ITestMethodTagDecorator> decorators, string tagName, TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            return decorators.Where(decorator => decorator.CanDecorateFrom(tagName, generationContext, testMethod));
        }

        private IEnumerable<ITestClassDecorator> FindDecorators(List<ITestClassDecorator> decorators, TestClassGenerationContext generationContext)
        {
            return decorators.Where(decorator => decorator.CanDecorateFrom(generationContext));
        }

        private IEnumerable<ITestClassTagDecorator> FindDecorators(List<ITestClassTagDecorator> decorators, string tagName, TestClassGenerationContext generationContext)
        {
            return decorators.Where(decorator => decorator.CanDecorateFrom(tagName, generationContext));
        }
    }
}