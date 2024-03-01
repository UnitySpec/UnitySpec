using System.Linq;
using System;
using UnityFlow.Generator.CodeDom;
using UnityFlow.Generator.UnitTestConverter;
using UnityFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;
using TechTalk.SpecFlow.Tracing;
using TechTalk.SpecFlow.Configuration;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace UnityFlow.Generator.Generation
{
    public class UnitTestFeatureGenerator : IFeatureGenerator
    {
        private readonly CodeDomHelper _codeDomHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly UnitTestMethodGenerator _unitTestMethodGenerator;
        private readonly LinePragmaHandler _linePragmaHandler;

        public UnitTestFeatureGenerator(
            IUnitTestGeneratorProvider testGeneratorProvider,
            CodeDomHelper codeDomHelper,
            SpecFlowConfiguration specFlowConfiguration,
            IDecoratorRegistry decoratorRegistry)
        {
            _testGeneratorProvider = testGeneratorProvider;
            _codeDomHelper = codeDomHelper;
            _specFlowConfiguration = specFlowConfiguration;
            _decoratorRegistry = decoratorRegistry;
            _linePragmaHandler = new LinePragmaHandler(_specFlowConfiguration, _codeDomHelper);
            _scenarioPartHelper = new ScenarioPartHelper(_specFlowConfiguration, _codeDomHelper);
            _unitTestMethodGenerator = new UnitTestMethodGenerator(testGeneratorProvider, decoratorRegistry, _codeDomHelper, _scenarioPartHelper, _specFlowConfiguration);
        }

        public string TestClassNameFormat { get; set; } = "{0}Feature";

        public NamespaceDeclarationSyntax GenerateUnitTestFixture(SpecFlowDocument document, string testClassName, string targetNamespace)
        {
            var codeNamespace = CreateNamespace(targetNamespace);
            var feature = document.SpecFlowFeature;

            testClassName = testClassName ?? string.Format(TestClassNameFormat, feature.Name.ToIdentifier());
            var generationContext = CreateTestClassStructure(codeNamespace, testClassName, document);

            SetupTestClass(generationContext);
            SetupTestClassInitializeMethod(generationContext);
            SetupTestClassCleanupMethod(generationContext);

            SetupScenarioStartMethod(generationContext);
            SetupScenarioInitializeMethod(generationContext);
            _scenarioPartHelper.SetupFeatureBackground(generationContext);
            SetupScenarioCleanupMethod(generationContext);

            SetupTestInitializeMethod(generationContext);
            SetupTestCleanupMethod(generationContext);

            _unitTestMethodGenerator.CreateUnitTests(feature, generationContext);

            //before returning the generated code, call the provider's method in case the generated code needs to be customized            
            _testGeneratorProvider.FinalizeTestClass(generationContext);
            return codeNamespace;
        }


        private TestClassGenerationContext CreateTestClassStructure(NamespaceDeclarationSyntax codeNamespace, string testClassName, SpecFlowDocument document)
        {
            var testClass = _codeDomHelper.CreateGeneratedTypeDeclaration(testClassName);
            codeNamespace.Members.Add(testClass);

            return new TestClassGenerationContext(
                _testGeneratorProvider,
                document,
                codeNamespace,
                testClass,
                DeclareTestRunnerMember(testClass),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.TESTCLASS_INITIALIZE_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.TESTCLASS_CLEANUP_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.TEST_INITIALIZE_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.TEST_CLEANUP_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.SCENARIO_INITIALIZE_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.SCENARIO_START_NAME),
                _codeDomHelper.CreateMethod(testClass, GeneratorConstants.SCENARIO_CLEANUP_NAME),
                document.SpecFlowFeature.HasFeatureBackground() ? _codeDomHelper.CreateMethod(testClass, GeneratorConstants.BACKGROUND_NAME) : null,
                _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _specFlowConfiguration.AllowRowTests);
        }

        private NamespaceDeclarationSyntax CreateNamespace(string targetNamespace)
        {
            targetNamespace = targetNamespace ?? GeneratorConstants.DEFAULT_NAMESPACE;

            if (!targetNamespace.StartsWith("global", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (_codeDomHelper.TargetLanguage)
                {
                    case CodeDomProviderLanguage.VB:
                        targetNamespace = $"GlobalVBNetNamespace.{targetNamespace}";
                        break;
                }
            }

            var codeNamespace = NamespaceDeclaration(_codeDomHelper.GetName(targetNamespace));

            codeNamespace.Usings.Add(_codeDomHelper.GetUsing(GeneratorConstants.UNITYFLOW_NAMESPACE));
            codeNamespace.Usings.Add(_codeDomHelper.GetUsing("System"));
            codeNamespace.Usings.Add(_codeDomHelper.GetUsing("System.Linq"));
            return codeNamespace;
        }

        private void SetupScenarioCleanupMethod(TestClassGenerationContext generationContext)
        {
            var scenarioCleanupMethod = generationContext.ScenarioCleanupMethod;

            scenarioCleanupMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            //testRunner.CollectScenarioErrors();
            scenarioCleanupMethod.Body.Statements.Add(ExpressionStatement(
                    InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.CollectScenarioErrors"))
                ));
        }

        private void SetupTestClass(TestClassGenerationContext generationContext)
        {
            generationContext.TestClass.Modifiers.Add(Token(SyntaxKind.PartialKeyword));
            generationContext.TestClass.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            _linePragmaHandler.AddLinePragmaInitial(generationContext.TestClass, generationContext.Document.SourceFilePath);

            _testGeneratorProvider.SetTestClass(generationContext, generationContext.Feature.Name, generationContext.Feature.Description);

            _decoratorRegistry.DecorateTestClass(generationContext, out var featureCategories);

            if (featureCategories.Any())
            {
                _testGeneratorProvider.SetTestClassCategories(generationContext, featureCategories);
            }

            var featureTagsField = FieldDeclaration(
                                        VariableDeclaration(
                                            _codeDomHelper.StringArray(OmittedArraySizeExpression())
                                            //ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                            //.WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                                        )
                                        .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(Identifier(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME))
                                                .WithInitializer(EqualsValueClause(_scenarioPartHelper.GetStringArrayExpression(generationContext.Feature.Tags)))
                                                )
                                            )
                                    );
            featureTagsField.Modifiers.AddRange(new[]{Token(SyntaxKind.PrivateKeyword),Token(SyntaxKind.StaticKeyword)});

            generationContext.TestClass.Members.Add(featureTagsField);
        }

        private FieldDeclarationSyntax DeclareTestRunnerMember(ClassDeclarationSyntax outerClass)
        {
            var testRunnerField = FieldDeclaration(
                VariableDeclaration(_codeDomHelper.GetName("UnityFlow.ITestRunner"))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GeneratorConstants.TESTRUNNER_FIELD))))
                ).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

            outerClass.Members.Add(testRunnerField);
            return testRunnerField;
        }


        private void SetupTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testClassInitializeMethod = generationContext.TestClassInitializeMethod;

            testClassInitializeMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));
            //testClassInitializeMethod.Name = GeneratorConstants.TESTCLASS_INITIALIZE_NAME;

            _testGeneratorProvider.SetTestClassInitializeMethod(generationContext);

            //testRunner = TestRunnerManager.GetTestRunner(); if UnitTestGeneratorTraits.ParallelExecution
            //testRunner = TestRunnerManager.GetTestRunner(null, 0); if not UnitTestGeneratorTraits.ParallelExecution
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            var testRunnerInvocation = InvocationExpression(_codeDomHelper.GetMemberAccess("UnityFlow.TestRunnerManager.GetTestRunner"));
            testRunnerInvocation = _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.ParallelExecution)
                ? testRunnerInvocation
                : testRunnerInvocation
                    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        Token(SyntaxKind.CommaToken),
                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))
                    })));

            testClassInitializeMethod.Body.AddStatements(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    testRunnerField,
                    testRunnerInvocation
                    )
                ));

            //FeatureInfo featureInfo = new FeatureInfo("xxxx");
            var arguments = _codeDomHelper.GetArgumentList(
                ObjectCreationExpression(
                    _codeDomHelper.GetName("System.Globalization.CultureInfo")
                    )
                    .WithArgumentList(_codeDomHelper.GetArgumentList(_codeDomHelper.StringLiteral(generationContext.Feature.Language))),
                _codeDomHelper.StringLiteral(generationContext.Document.DocumentLocation?.FeatureFolderPath),
                _codeDomHelper.StringLiteral(generationContext.Feature.Name),
                _codeDomHelper.StringLiteral(generationContext.Feature.Description),
                _codeDomHelper.GetMemberAccess(_codeDomHelper.TargetLanguage.ToString()),
                _codeDomHelper.GetName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME)
                );
            var featureInfoDecl = VariableDeclaration(_codeDomHelper.GetName("UnityFlow.FeatureInfo"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("featureInfo"))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(_codeDomHelper.GetName("UnitFlow.FeatureInfo"))
                        .WithArgumentList(arguments)
                    ))
                ));

            testClassInitializeMethod.Body.AddStatements(LocalDeclarationStatement(featureInfoDecl));

            //testRunner.OnFeatureStart(featureInfo);
            testClassInitializeMethod.Body.AddStatements(ExpressionStatement(
                InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnFeatureStart")) 
                .WithArgumentList(_codeDomHelper.GetArgumentList(IdentifierName("featureInfo")))
                ));
        }


        private void SetupTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testClassCleanupMethod = generationContext.TestClassCleanupMethod;

            testClassCleanupMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestClassCleanupMethod(generationContext);

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();
            //            testRunner.OnFeatureEnd();
            testClassCleanupMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnFeatureEnd"))
                ));
            //            testRunner = null;
            testClassCleanupMethod.Body.Statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, 
                    testRunnerField, 
                    LiteralExpression(SyntaxKind.NullLiteralExpression)
                    )
                ));
        }

        private void SetupTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testInitializeMethod = generationContext.TestInitializeMethod;

            testInitializeMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestInitializeMethod(generationContext);
        }

        private void SetupTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testCleanupMethod = generationContext.TestCleanupMethod;

            testCleanupMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestCleanupMethod(generationContext);

            //testRunner.OnScenarioEnd();
            testCleanupMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioEnd"))
                )); 
        }

        private void SetupScenarioInitializeMethod(TestClassGenerationContext generationContext)
        {
            var scenarioInitializeMethod = generationContext.ScenarioInitializeMethod;

            scenarioInitializeMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));
            scenarioInitializeMethod.ParameterList.AddParameters(
                    Parameter(Identifier("scenarioInfo"))
                    .WithType(_codeDomHelper.GetName("UnityFlow.ScenarioInfo"))
                );

            //testRunner.OnScenarioInitialize(scenarioInfo);
            scenarioInitializeMethod.Body.AddStatements(ExpressionStatement(
                InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioInitialize"))
                .WithArgumentList(_codeDomHelper.GetArgumentList(IdentifierName("scenarioInfo")))
                ));
        }

        private void SetupScenarioStartMethod(TestClassGenerationContext generationContext)
        {
            var scenarioStartMethod = generationContext.ScenarioStartMethod;

            scenarioStartMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            //testRunner.OnScenarioStart();
            scenarioStartMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioStart"))
                )); 
        }
    }
}