using System.Collections.Generic;
using UnityFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace UnityFlow.Generator
{
    public class TestClassGenerationContext
    {
        public IUnitTestGeneratorProvider UnitTestGeneratorProvider { get; private set; }
        public SpecFlowDocument Document { get; private set; }

        public SpecFlowFeature Feature => Document.SpecFlowFeature;

        public NamespaceDeclarationSyntax Namespace { get; private set; }
        public ClassDeclarationSyntax TestClass { get; private set; }
        public MethodDeclarationSyntax TestClassInitializeMethod { get; private set; }
        public MethodDeclarationSyntax TestClassCleanupMethod { get; private set; }
        public MethodDeclarationSyntax TestInitializeMethod { get; private set; }
        public MethodDeclarationSyntax TestCleanupMethod { get; private set; }
        public MethodDeclarationSyntax ScenarioInitializeMethod { get; private set; }
        public MethodDeclarationSyntax ScenarioStartMethod { get; private set; }
        public MethodDeclarationSyntax ScenarioCleanupMethod { get; private set; }
        public MethodDeclarationSyntax FeatureBackgroundMethod { get; private set; }
        public FieldDeclarationSyntax TestRunnerField { get; private set; }

        public bool GenerateRowTests { get; private set; }

        public IDictionary<string, object> CustomData { get; private set; }

        public TestClassGenerationContext(
            IUnitTestGeneratorProvider unitTestGeneratorProvider,
            SpecFlowDocument document,
            NamespaceDeclarationSyntax ns,
            ClassDeclarationSyntax testClass,
            FieldDeclarationSyntax testRunnerField,
            MethodDeclarationSyntax testClassInitializeMethod,
            MethodDeclarationSyntax testClassCleanupMethod,
            MethodDeclarationSyntax testInitializeMethod,
            MethodDeclarationSyntax testCleanupMethod,
            MethodDeclarationSyntax scenarioInitializeMethod,
            MethodDeclarationSyntax scenarioStartMethod,
            MethodDeclarationSyntax scenarioCleanupMethod,
            MethodDeclarationSyntax featureBackgroundMethod,
            bool generateRowTests)
        {
            UnitTestGeneratorProvider = unitTestGeneratorProvider;
            Document = document;
            Namespace = ns;
            TestClass = testClass;
            TestRunnerField = testRunnerField;
            TestClassInitializeMethod = testClassInitializeMethod;
            TestClassCleanupMethod = testClassCleanupMethod;
            TestInitializeMethod = testInitializeMethod;
            TestCleanupMethod = testCleanupMethod;
            ScenarioInitializeMethod = scenarioInitializeMethod;
            ScenarioStartMethod = scenarioStartMethod;
            ScenarioCleanupMethod = scenarioCleanupMethod;
            FeatureBackgroundMethod = featureBackgroundMethod;
            GenerateRowTests = generateRowTests;

            CustomData = new Dictionary<string, object>();
        }
    }
}