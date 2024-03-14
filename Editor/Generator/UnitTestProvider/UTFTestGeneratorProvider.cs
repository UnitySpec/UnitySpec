using BoDi;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitySpec.Generator.Generation;
using UnitySpec.Generator.Roslyn;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnitySpec.Generator.UnitTestProvider
{
    public class UTFTestGeneratorProvider : IUnitTestGeneratorProvider
    {
        protected internal const string TESTFIXTURESETUP_ATTR_NUNIT3 = "NUnit.Framework.OneTimeSetUpAttribute";
        protected internal const string TESTFIXTURETEARDOWN_ATTR_NUNIT3 = "NUnit.Framework.OneTimeTearDownAttribute";
        protected internal const string NONPARALLELIZABLE_ATTR = "NUnit.Framework.NonParallelizableAttribute";
        protected internal const string TESTFIXTURE_ATTR = "NUnit.Framework.TestFixtureAttribute";
        protected internal const string TEST_ATTR = "UnityEngine.TestTools.UnityTestAttribute";
        protected internal const string ROW_ATTR = "NUnit.Framework.TestCaseAttribute";
        protected internal const string CATEGORY_ATTR = "NUnit.Framework.CategoryAttribute";
        protected internal const string TESTSETUP_ATTR = "NUnit.Framework.SetUpAttribute";
        protected internal const string TESTTEARDOWN_ATTR = "NUnit.Framework.TearDownAttribute";
        protected internal const string IGNORE_ATTR = "NUnit.Framework.IgnoreAttribute";
        protected internal const string DESCRIPTION_ATTR = "NUnit.Framework.DescriptionAttribute";
        protected internal const string TESTCONTEXT_TYPE = "NUnit.Framework.TestContext";
        protected internal const string TESTCONTEXT_INSTANCE = "NUnit.Framework.TestContext.CurrentContext";

        public UTFTestGeneratorProvider(RoslynHelper roslynHelper)
        {
            this.roslynHelper = roslynHelper;
        }

        protected RoslynHelper roslynHelper { get; set; }

        public bool GenerateParallelCodeForFeature { get; set; }

        public virtual UnitTestGeneratorTraits GetTraits()
        {
            return UnitTestGeneratorTraits.None;
        }

        public virtual void SetTestClassIgnore(TestClassGenerationContext generationContext)
        {
            generationContext.TestClass = generationContext.TestClass.AddAttributeLists(
                roslynHelper.getAttribute(IGNORE_ATTR, "Ignored feature")
                );
        }

        public virtual MethodDeclarationSyntax SetTestMethodIgnore(MethodDeclarationSyntax testMethod)
        {
            return testMethod.AddAttributeLists(
                    roslynHelper.getAttribute(IGNORE_ATTR, "Ignored scenario")
                );
        }

        public virtual void SetTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassInitializeMethod = generationContext.TestClassInitializeMethod.AddAttributeLists(
                        roslynHelper.getAttribute(TESTFIXTURESETUP_ATTR_NUNIT3)
                    );
        }

        public virtual void SetTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestClassCleanupMethod = generationContext.TestClassCleanupMethod.AddAttributeLists(
                    roslynHelper.getAttribute(TESTFIXTURETEARDOWN_ATTR_NUNIT3)
                );
        }

        public virtual void SetTestClassNonParallelizable(TestClassGenerationContext generationContext)
        {
            generationContext.TestClass = generationContext.TestClass.AddAttributeLists(
                    roslynHelper.getAttribute(NONPARALLELIZABLE_ATTR)
                    );
        }

        public void SetTestClass(TestClassGenerationContext generationContext, string featureTitle, string featureDescription)
        {
            generationContext.TestClass = generationContext.TestClass.AddAttributeLists(
                        roslynHelper.getAttribute(TESTFIXTURE_ATTR),
                        roslynHelper.getAttribute(DESCRIPTION_ATTR, featureTitle)
                );
        }

        public void SetTestClassCategories(TestClassGenerationContext generationContext, IEnumerable<string> featureCategories)
        {
            generationContext.TestClass = generationContext.TestClass.AddAttributeLists(
                roslynHelper.getAttributeForEachValue(CATEGORY_ATTR, featureCategories).ToArray()
                );
        }

        public virtual void FinalizeTestClass(TestClassGenerationContext generationContext)
        {
            // testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
            var invocation =
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        roslynHelper.GetMemberAccess(
                            "ScenarioContainer",
                            "ScenarioContext",
                            GeneratorConstants.TESTRUNNER_FIELD
                            ),
                        GenericName(Identifier(nameof(IObjectContainer.RegisterInstanceAs)))
                        .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList<TypeSyntax>(
                            roslynHelper.GetName(TESTCONTEXT_TYPE)
                            )))
                        )
                    ).WithArgumentList(
                        roslynHelper.GetArgumentList(
                            roslynHelper.GetMemberAccess(TESTCONTEXT_INSTANCE)
                            )
                        );
            generationContext.ScenarioInitializeMethod = generationContext.ScenarioInitializeMethod.WithBody(
                generationContext.ScenarioInitializeMethod.Body.AddStatements(ExpressionStatement(invocation))
                );
        }

        public void SetTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestInitializeMethod = generationContext.TestInitializeMethod.AddAttributeLists(roslynHelper.getAttribute(TESTSETUP_ATTR));
        }

        public void SetTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            generationContext.TestCleanupMethod = generationContext.TestCleanupMethod.AddAttributeLists(roslynHelper.getAttribute(TESTTEARDOWN_ATTR));
        }

        public MethodDeclarationSyntax MakeTestMethod(MethodDeclarationSyntax testMethod, string friendlyTestName)
        {
            return testMethod.AddAttributeLists(
                new AttributeListSyntax[]
                    {
                        roslynHelper.getAttribute(TEST_ATTR),
                        roslynHelper.getAttribute(DESCRIPTION_ATTR, friendlyTestName)
                    }
                );
        }

        public MethodDeclarationSyntax SetTestMethodCategories(MethodDeclarationSyntax testMethod, IEnumerable<string> scenarioCategories)
        {
            return testMethod.AddAttributeLists(
                    roslynHelper.getAttributeForEachValue(CATEGORY_ATTR, scenarioCategories).ToArray()
                    );
        }

        public void SetRowTest(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, string scenarioTitle)
        {
            //Debug.LogError("Row tests are not supported in Unity");
            throw new NotSupportedException();
            //MakeTestMethod(generationContext, testMethod, scenarioTitle);

        }

        public void SetRow(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, IEnumerable<string> arguments, IEnumerable<string> tags, bool isIgnored)
        {
            //Debug.LogError("Row tests are not supported in Unity");
            throw new NotSupportedException();

            //var args = arguments.Select(
            //    arg => new CodeAttributeArgument(new CodePrimitiveExpression(arg))).ToList();

            //// addressing ReSharper bug: TestCase attribute with empty string[] param causes inconclusive result - https://github.com/techtalk/SpecFlow/issues/116
            //bool hasExampleTags = tags.Any();
            //var exampleTagExpressionList = tags.Select(t => new CodePrimitiveExpression(t));
            //var exampleTagsExpression = hasExampleTags
            //    ? new CodeArrayCreateExpression(typeof(string[]), exampleTagExpressionList.ToArray())
            //    : (CodeExpression)new CodePrimitiveExpression(null);

            //args.Add(new CodeAttributeArgument(exampleTagsExpression));

            //// adds 'Category' named parameter so that NUnit also understands that this test case belongs to the given categories
            //if (hasExampleTags)
            //{
            //    CodeExpression exampleTagsStringExpr = new CodePrimitiveExpression(string.Join(",", tags.ToArray()));
            //    args.Add(new CodeAttributeArgument("Category", exampleTagsStringExpr));
            //}

            //if (isIgnored)
            //    args.Add(new CodeAttributeArgument("Ignored", new CodePrimitiveExpression(true)));

            //RoslynHelper.getAttribute(testMethod, ROW_ATTR, args.ToArray());
        }

        public void SetTestMethodAsRow(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod, string scenarioTitle, string exampleSetName, string variantName, IEnumerable<KeyValuePair<string, string>> arguments)
        {
            // No additional setup needed
            //string readableTestName = $"{scenarioTitle} with parameters: ";
            //foreach (var pair in arguments) {
            //    readableTestName += $"({pair.Key}: {pair.Value})";
            //}

        }
    }
}
