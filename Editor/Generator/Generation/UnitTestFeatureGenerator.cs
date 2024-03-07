using System.Linq;
using System;
using UnityFlow.Generator.Roslyn;
using UnityFlow.Generator.UnitTestConverter;
using UnityFlow.Generator.UnitTestProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using UnityFlow.General.Configuration;
using UnityFlow.General.Parser;
using UnityFlow.General.Extensions;

namespace UnityFlow.Generator.Generation
{
    public class UnitTestFeatureGenerator : IFeatureGenerator
    {
        private readonly RoslynHelper _roslynHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly IUnitTestGeneratorProvider _testGeneratorProvider;
        private readonly UnitTestMethodGenerator _unitTestMethodGenerator;
        private readonly LinePragmaHandler _linePragmaHandler;

        public UnitTestFeatureGenerator(
            IUnitTestGeneratorProvider testGeneratorProvider,
            RoslynHelper roslynHelper,
            SpecFlowConfiguration specFlowConfiguration,
            IDecoratorRegistry decoratorRegistry)
        {
            _testGeneratorProvider = testGeneratorProvider;
            _roslynHelper = roslynHelper;
            _specFlowConfiguration = specFlowConfiguration;
            _decoratorRegistry = decoratorRegistry;
            _linePragmaHandler = new LinePragmaHandler(_specFlowConfiguration, _roslynHelper);
            _scenarioPartHelper = new ScenarioPartHelper(_specFlowConfiguration, _roslynHelper);
            _unitTestMethodGenerator = new UnitTestMethodGenerator(testGeneratorProvider, decoratorRegistry, _roslynHelper, _scenarioPartHelper, _specFlowConfiguration);
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

            return generationContext.BuildClass();
        }


        private TestClassGenerationContext CreateTestClassStructure(NamespaceDeclarationSyntax codeNamespace, string testClassName, SpecFlowDocument document)
        {
            return new TestClassGenerationContext(
                _testGeneratorProvider,
                document,
                codeNamespace,
                _roslynHelper.CreateGeneratedTypeDeclaration(testClassName),
                DeclareTestRunnerMember(),
                _roslynHelper.CreateMethod(GeneratorConstants.TESTCLASS_INITIALIZE_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.TESTCLASS_CLEANUP_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.TEST_INITIALIZE_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.TEST_CLEANUP_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.SCENARIO_INITIALIZE_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.SCENARIO_START_NAME),
                _roslynHelper.CreateMethod(GeneratorConstants.SCENARIO_CLEANUP_NAME),
                document.SpecFlowFeature.HasFeatureBackground() ? _roslynHelper.CreateMethod(GeneratorConstants.BACKGROUND_NAME) : null,
                _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.RowTests) && _specFlowConfiguration.AllowRowTests);
        }

        private NamespaceDeclarationSyntax CreateNamespace(string targetNamespace)
        {
            targetNamespace = targetNamespace ?? GeneratorConstants.DEFAULT_NAMESPACE;

            if (!targetNamespace.StartsWith("global", StringComparison.CurrentCultureIgnoreCase))
            {
                switch (_roslynHelper.TargetLanguage)
                {
                    case ProviderLanguage.VB:
                        targetNamespace = $"GlobalVBNetNamespace.{targetNamespace}";
                        break;
                }
            }
            var usings = List<UsingDirectiveSyntax>(new UsingDirectiveSyntax[]
            {
                _roslynHelper.GetUsing(GeneratorConstants.UNITYFLOW_NAMESPACE),
                _roslynHelper.GetUsing("System"),
                _roslynHelper.GetUsing("System.Linq")
            });
            var codeNamespace = NamespaceDeclaration(_roslynHelper.GetName(targetNamespace)).WithUsings(usings);

            return codeNamespace;
        }

        private void SetupScenarioCleanupMethod(TestClassGenerationContext generationContext)
        {
            var scenarioCleanupMethod = generationContext.ScenarioCleanupMethod;

            var mods = scenarioCleanupMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            //testRunner.CollectScenarioErrors();
            var statements =scenarioCleanupMethod.Body.Statements.Add(ExpressionStatement(
                    InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.CollectScenarioErrors"))
                ));

            generationContext.ScenarioCleanupMethod = generationContext.ScenarioCleanupMethod.WithModifiers(mods).WithBody(Block(statements));
        }

        private void SetupTestClass(TestClassGenerationContext generationContext)
        {
            var modifiers = new SyntaxTokenList{Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.PartialKeyword)};

            _linePragmaHandler.AddLinePragmaInitial(generationContext.TestClass, generationContext.Document.SourceFilePath); // TODO

            _testGeneratorProvider.SetTestClass(generationContext, generationContext.Feature.Name, generationContext.Feature.Description);

            _decoratorRegistry.DecorateTestClass(generationContext, out var featureCategories);

            if (featureCategories.Any())
            {
                _testGeneratorProvider.SetTestClassCategories(generationContext, featureCategories);
            }

            var featureTagsField = FieldDeclaration(
                                        VariableDeclaration(
                                            _roslynHelper.StringArray(OmittedArraySizeExpression())
                                            //ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                                            //.WithRankSpecifiers(SingletonList(ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))))
                                        )
                                        .WithVariables(
                                            SingletonSeparatedList(
                                                VariableDeclarator(Identifier(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME))
                                                .WithInitializer(EqualsValueClause(_scenarioPartHelper.GetStringArrayExpression(generationContext.Feature.Tags)))
                                                )
                                            )
                                    )
                                    .WithModifiers(new SyntaxTokenList( Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.StaticKeyword) ));

            generationContext.TestClass = generationContext.TestClass
                .WithModifiers(generationContext.TestClass.Modifiers.AddRange(modifiers))
                .WithMembers(generationContext.TestClass.Members.Add(featureTagsField));
        }

        private FieldDeclarationSyntax DeclareTestRunnerMember()
        {
            var testRunnerField = FieldDeclaration(
                VariableDeclaration(_roslynHelper.GetName("UnityFlow.ITestRunner"))
                    .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GeneratorConstants.TESTRUNNER_FIELD))))
                ).WithModifiers(TokenList(Token(SyntaxKind.PrivateKeyword)));

            return testRunnerField;
        }


        private void SetupTestClassInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testClassInitializeMethod = generationContext.TestClassInitializeMethod;
            var mods = testClassInitializeMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestClassInitializeMethod(generationContext);

            //testRunner = TestRunnerManager.GetTestRunner(); if UnitTestGeneratorTraits.ParallelExecution
            //testRunner = TestRunnerManager.GetTestRunner(null, 0); if not UnitTestGeneratorTraits.ParallelExecution
            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();

            var testRunnerInvocation = InvocationExpression(_roslynHelper.GetMemberAccess("UnityFlow.TestRunnerManager.GetTestRunner"));
            testRunnerInvocation = _testGeneratorProvider.GetTraits().HasFlag(UnitTestGeneratorTraits.ParallelExecution)
                ? testRunnerInvocation
                : testRunnerInvocation
                    .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrToken[]
                    {
                        Argument(LiteralExpression(SyntaxKind.NullLiteralExpression)),
                        Token(SyntaxKind.CommaToken),
                        Argument(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))
                    })));

            var statements = testClassInitializeMethod.Body.Statements.ToList();
                
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    testRunnerField,
                    testRunnerInvocation
                    )
                ));

            //FeatureInfo featureInfo = new FeatureInfo("xxxx");
            var arguments = _roslynHelper.GetArgumentList(
                ObjectCreationExpression(
                    _roslynHelper.GetName("System.Globalization.CultureInfo")
                    )
                    .WithArgumentList(_roslynHelper.GetArgumentList(_roslynHelper.StringLiteral(generationContext.Feature.Language))),
                _roslynHelper.StringLiteral(generationContext.Document.DocumentLocation?.FeatureFolderPath),
                _roslynHelper.StringLiteral(generationContext.Feature.Name),
                _roslynHelper.StringLiteral(generationContext.Feature.Description),
                _roslynHelper.GetMemberAccess(_roslynHelper.TargetLanguage.GetLanguage()),
                _roslynHelper.GetName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME)
                );
            var featureInfoDecl = VariableDeclaration(_roslynHelper.GetName("UnityFlow.FeatureInfo"))
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier("featureInfo"))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(_roslynHelper.GetName("UnityFlow.FeatureInfo"))
                        .WithArgumentList(arguments)
                    ))
                ));

            statements.Add(LocalDeclarationStatement(featureInfoDecl));

            //testRunner.OnFeatureStart(featureInfo);
            statements.Add(ExpressionStatement(
                InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnFeatureStart")) 
                .WithArgumentList(_roslynHelper.GetArgumentList(IdentifierName("featureInfo")))
                ));

            generationContext.TestClassInitializeMethod = 
                generationContext.TestClassInitializeMethod
                .WithModifiers(mods)
                .WithBody(Block(statements));
        }


        private void SetupTestClassCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testClassCleanupMethod = generationContext.TestClassCleanupMethod;

            testClassCleanupMethod = testClassCleanupMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestClassCleanupMethod(generationContext);

            var testRunnerField = _scenarioPartHelper.GetTestRunnerExpression();
            var statements = testClassCleanupMethod.Body.Statements.ToList();
            //            testRunner.OnFeatureEnd();
            statements.Add(ExpressionStatement(
                InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnFeatureEnd"))
                ));
            //            testRunner = null;
            statements.Add(ExpressionStatement(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, 
                    testRunnerField, 
                    LiteralExpression(SyntaxKind.NullLiteralExpression)
                    )
                ));

            generationContext.TestClassCleanupMethod = testClassCleanupMethod.WithBody(Block(statements));
        }

        private void SetupTestInitializeMethod(TestClassGenerationContext generationContext)
        {
            var testInitializeMethod = generationContext.TestInitializeMethod;

            var mods = testInitializeMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestInitializeMethod(generationContext);

            generationContext.TestInitializeMethod = generationContext.TestInitializeMethod.WithModifiers(mods);
        }

        private void SetupTestCleanupMethod(TestClassGenerationContext generationContext)
        {
            var testCleanupMethod = generationContext.TestCleanupMethod;

            var mods = testCleanupMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            _testGeneratorProvider.SetTestCleanupMethod(generationContext);

            //testRunner.OnScenarioEnd();
            var statements = generationContext.TestCleanupMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioEnd"))
                )); 
            generationContext.TestCleanupMethod = generationContext.TestCleanupMethod.WithModifiers(mods).WithBody(Block(statements));
        }

        private void SetupScenarioInitializeMethod(TestClassGenerationContext generationContext)
        {
            var scenarioInitializeMethod = generationContext.ScenarioInitializeMethod;

            var mods = scenarioInitializeMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));
            var parList = scenarioInitializeMethod.ParameterList.Parameters.Add(
                    Parameter(Identifier("scenarioInfo"))
                    .WithType(_roslynHelper.GetName("UnityFlow.ScenarioInfo"))
                );

            //testRunner.OnScenarioInitialize(scenarioInfo);
            var statements = scenarioInitializeMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioInitialize"))
                .WithArgumentList(_roslynHelper.GetArgumentList(IdentifierName("scenarioInfo")))
                ));
            generationContext.ScenarioInitializeMethod = 
                generationContext.ScenarioInitializeMethod
                .WithModifiers(mods)
                .WithParameterList(ParameterList(parList))
                .WithBody(Block(statements));
        }

        private void SetupScenarioStartMethod(TestClassGenerationContext generationContext)
        {
            var scenarioStartMethod = generationContext.ScenarioStartMethod;

            var mods = scenarioStartMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));

            //testRunner.OnScenarioStart();
            var statements = scenarioStartMethod.Body.Statements.Add(ExpressionStatement(
                InvocationExpression(_roslynHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.OnScenarioStart"))
                )); 

            generationContext.ScenarioStartMethod = generationContext.ScenarioStartMethod.WithModifiers(mods).WithBody(Block(statements));
        }
    }
}