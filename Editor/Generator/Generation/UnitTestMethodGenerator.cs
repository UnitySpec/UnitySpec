using Gherkin.Ast;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using UnitySpec.General.Configuration;
using UnitySpec.General.Extensions;
using UnitySpec.General.Parser;
using UnitySpec.Generator.Roslyn;
using UnitySpec.Generator.UnitTestConverter;
using UnitySpec.Generator.UnitTestProvider;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace UnitySpec.Generator.Generation
{
    public class UnitTestMethodGenerator
    {
        private const string IGNORE_TAG = "@Ignore";
        private const string TESTRUNNER_FIELD = "testRunner";
        private readonly RoslynHelper _roslynHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly IUnitTestGeneratorProvider _unitTestGeneratorProvider;

        public UnitTestMethodGenerator(
            IUnitTestGeneratorProvider unitTestGeneratorProvider,
            IDecoratorRegistry decoratorRegistry,
            RoslynHelper roslynHelper,
            ScenarioPartHelper scenarioPartHelper,
            SpecFlowConfiguration specFlowConfiguration)
        {
            _unitTestGeneratorProvider = unitTestGeneratorProvider;
            _decoratorRegistry = decoratorRegistry;
            _roslynHelper = roslynHelper;
            _scenarioPartHelper = scenarioPartHelper;
            _specFlowConfiguration = specFlowConfiguration;
        }

        public void CreateUnitTests(SpecFlowFeature feature, TestClassGenerationContext generationContext)
        {
            foreach (var scenarioDefinition in feature.ScenarioDefinitions)
            {
                CreateUnitTest(feature, generationContext, scenarioDefinition);
            }
        }

        private void CreateUnitTest(SpecFlowFeature feature, TestClassGenerationContext generationContext, StepsContainer scenarioDefinition)
        {
            if (string.IsNullOrEmpty(scenarioDefinition.Name))
            {
                throw new TestGeneratorException("The scenario must have a title specified.");
            }

            if (scenarioDefinition is ScenarioOutline scenarioOutline)
            {
                GenerateScenarioOutlineTest(generationContext, scenarioOutline, feature);
            }
            else
            {
                GenerateTest(generationContext, (Scenario)scenarioDefinition, feature);
            }
        }

        private void GenerateScenarioOutlineTest(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, SpecFlowFeature feature)
        {
            ValidateExampleSetConsistency(scenarioOutline);

            var paramToIdentifier = CreateParamToIdentifierMapping(scenarioOutline);

            var scenarioOutlineTestMethod = CreateScenatioOutlineTestMethod(generationContext, scenarioOutline, paramToIdentifier);
            var exampleTagsParam = IdentifierName(GeneratorConstants.SCENARIO_OUTLINE_EXAMPLE_TAGS_PARAMETER);
            // Unity doesn't support rowtests at the moment
            //if (generationContext.GenerateRowTests)
            //{
            //    GenerateScenarioOutlineExamplesAsRowTests(generationContext, scenarioOutline, scenarioOutlineTestMethod);
            //}
            //else
            //{
            GenerateScenarioOutlineExamplesAsIndividualMethods(scenarioOutline, generationContext, scenarioOutlineTestMethod, paramToIdentifier);
            //}

            scenarioOutlineTestMethod = GenerateTestBody(generationContext, scenarioOutline, scenarioOutlineTestMethod, feature, exampleTagsParam, paramToIdentifier);
            generationContext.TestClass = generationContext.TestClass.AddMembers(scenarioOutlineTestMethod);
        }

        private void GenerateTest(TestClassGenerationContext generationContext, Scenario scenario, SpecFlowFeature feature)
        {
            var testMethod = CreateTestMethod(generationContext, scenario, null);
            testMethod = GenerateTestBody(generationContext, scenario, testMethod, feature);
            generationContext.TestClass = generationContext.TestClass.AddMembers(testMethod);
        }

        private void ValidateExampleSetConsistency(ScenarioOutline scenarioOutline)
        {
            if (scenarioOutline.Examples.Count() <= 1)
            {
                return;
            }

            var firstExamplesHeader = scenarioOutline.Examples.First().TableHeader.Cells.Select(c => c.Value).ToArray();

            //check params
            if (scenarioOutline.Examples
                .Skip(1)
                .Select(examples => examples.TableHeader.Cells.Select(c => c.Value))
                .Any(paramNames => !paramNames.SequenceEqual(firstExamplesHeader)))
            {
                throw new TestGeneratorException("The example sets must provide the same parameters.");
            }
        }

        private IEnumerable<string> GetNonIgnoreTags(IEnumerable<Tag> tags)
        {
            return tags.Where(t => !t.Name.Equals(IGNORE_TAG, StringComparison.InvariantCultureIgnoreCase)).Select(t => t.GetNameWithoutAt());
        }

        private bool HasIgnoreTag(IEnumerable<Tag> tags)
        {
            return tags.Any(t => t.Name.Equals(IGNORE_TAG, StringComparison.InvariantCultureIgnoreCase));
        }

        private MethodDeclarationSyntax GenerateTestBody(
            TestClassGenerationContext generationContext,
            StepsContainer scenario,
            MethodDeclarationSyntax testMethod,
            SpecFlowFeature feature,
            ExpressionSyntax additionalTagsExpression = null, ParameterSubstitution paramToIdentifier = null)
        {
            //call test setup
            //ScenarioInfo scenarioInfo = new ScenarioInfo("xxxx", tags...);
            ExpressionSyntax tagsExpression;
            if (additionalTagsExpression == null)
            {
                tagsExpression = _scenarioPartHelper.GetStringArrayExpression(scenario.GetTags());
            }
            else if (!scenario.HasTags())
            {
                tagsExpression = additionalTagsExpression;
            }
            else
            {
                // merge tags list
                // var tags = tags1
                // if (tags2 != null)
                //   tags = Enumerable.ToArray(Enumerable.Concat(tags1, tags1));
                throw new NotImplementedException("Tags merging");
                //testMethod.Statements.Add(
                //    new CodeVariableDeclarationStatement(typeof(string[]), "__tags", _scenarioPartHelper.GetStringArrayExpression(scenario.GetTags())));
                //tagsExpression = new CodeVariableReferenceExpression("__tags");
                //testMethod.Statements.Add(
                //    new CodeConditionStatement(
                //        new CodeBinaryOperatorExpression(
                //            additionalTagsExpression,
                //            CodeBinaryOperatorType.IdentityInequality,
                //            new CodePrimitiveExpression(null)),
                //        new CodeAssignStatement(
                //            tagsExpression,
                //            new CodeMethodInvokeExpression(
                //                new CodeTypeReferenceExpression(typeof(Enumerable)),
                //                "ToArray",
                //                new CodeMethodInvokeExpression(
                //                    new CodeTypeReferenceExpression(typeof(Enumerable)),
                //                    "Concat",
                //                    tagsExpression,
                //                    additionalTagsExpression)))));
            }

            testMethod = AddVariableForTags(testMethod, tagsExpression);

            testMethod = AddVariableForArguments(testMethod, paramToIdentifier);

            var arguments = _roslynHelper.GetArgumentList(
                                _roslynHelper.StringLiteral(scenario.Name),
                                _roslynHelper.StringLiteral(scenario.Description),
                                _roslynHelper.GetName(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME),
                                _roslynHelper.GetName(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME),
                                _roslynHelper.GetName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME)
                                );
            var scenarioInfoDecl = VariableDeclaration(_roslynHelper.GetName("UnitySpec.ScenarioInfo"))
                                    .WithVariables(SingletonSeparatedList(
                                        VariableDeclarator(Identifier("scenarioInfo"))
                                        .WithInitializer(EqualsValueClause(
                                            ObjectCreationExpression(_roslynHelper.GetName("UnitySpec.ScenarioInfo"))
                                            .WithArgumentList(arguments)
                                        ))
                                    ));
            testMethod = testMethod.WithBody(testMethod.Body.AddStatements(LocalDeclarationStatement(scenarioInfoDecl)));

            testMethod = GenerateScenarioInitializeCall(scenario, testMethod);

            testMethod = GenerateTestMethodBody(generationContext, scenario, testMethod, paramToIdentifier, feature);

            testMethod = testMethod.WithBody(testMethod.Body.AddStatements(GenerateScenarioCleanupMethodCall(generationContext, testMethod)));
            return testMethod;

        }

        private MethodDeclarationSyntax AddVariableForTags(MethodDeclarationSyntax testMethod, ExpressionSyntax tagsExpression)
        {
            var tagVariable = VariableDeclaration(_roslynHelper.StringArray(OmittedArraySizeExpression()))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME))
                    .WithInitializer(EqualsValueClause(tagsExpression))
                    ));

            return testMethod.WithBody(testMethod.Body.AddStatements(LocalDeclarationStatement(tagVariable)));
        }

        private MethodDeclarationSyntax AddVariableForArguments(MethodDeclarationSyntax testMethod, ParameterSubstitution paramToIdentifier)
        {
            List<StatementSyntax> statements = new List<StatementSyntax>();
            var type = _roslynHelper.GetName("System.Collections.Specialized.OrderedDictionary");
            var variableDecl = VariableDeclaration(type)
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(type)
                        .WithArgumentList(ArgumentList())
                        ))
                    ));

            statements.Add(LocalDeclarationStatement(variableDecl));

            if (paramToIdentifier != null)
            {
                foreach (var parameter in paramToIdentifier)
                {
                    var addArgumentExpression = ExpressionStatement(
                        InvocationExpression(_roslynHelper.GetMemberAccess("argumentsOfScenario.Add"))
                        .WithArgumentList(_roslynHelper.GetArgumentList(
                            _roslynHelper.StringLiteral(parameter.Key),
                            IdentifierName(parameter.Value)
                            )
                        ));

                    statements.Add(addArgumentExpression);
                }
            }
            return testMethod.WithBody(testMethod.Body.AddStatements(statements.ToArray()));
        }

        internal MethodDeclarationSyntax GenerateTestMethodBody(TestClassGenerationContext generationContext, StepsContainer scenario, MethodDeclarationSyntax testMethod, ParameterSubstitution paramToIdentifier, SpecFlowFeature feature)
        {
            var statementsWhenScenarioIsIgnored = SingletonList(ExpressionStatement(CreateTestRunnerSkipScenarioCall()));
            var statementsWhenScenarioIsExecuted = new List<StatementSyntax>
            {
                ExpressionStatement(InvocationExpression(_roslynHelper.GetMemberAccess(ThisExpression(), "ScenarioStart")))
            };


            if (generationContext.Feature.HasFeatureBackground())
            {
                var thisExpr = _roslynHelper.AddSourceLinePragmaStatement(IdentifierName(GeneratorConstants.BACKGROUND_NAME), scenario.Location.Line);
                statementsWhenScenarioIsExecuted.Add(ExpressionStatement(InvocationExpression(thisExpr)));
            }


            foreach (var scenarioStep in scenario.Steps)
            {
                _scenarioPartHelper.GenerateStep(generationContext, statementsWhenScenarioIsExecuted, scenarioStep, paramToIdentifier);
            }

            var tagsOfScenarioVariableReferenceExpression = IdentifierName(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME);
            var featureFileTagFieldReferenceExpression = IdentifierName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME);

            var tagHelperReference = IdentifierName("TagHelper");
            var scenarioTagIgnoredCheckStatement = InvocationExpression(_roslynHelper.GetMemberAccess(tagHelperReference, "ContainsIgnoreTag"))
                                                    .WithArgumentList(_roslynHelper.GetArgumentList(tagsOfScenarioVariableReferenceExpression));
            var featureTagIgnoredCheckStatement = InvocationExpression(_roslynHelper.GetMemberAccess(tagHelperReference, "ContainsIgnoreTag"))
                                                    .WithArgumentList(_roslynHelper.GetArgumentList(featureFileTagFieldReferenceExpression));
            var conditionStatement = BinaryExpression(SyntaxKind.LogicalOrExpression, scenarioTagIgnoredCheckStatement, featureTagIgnoredCheckStatement);
            var ifElseStatement = IfStatement(
                                    ParenthesizedExpression(conditionStatement),
                                    Block(statementsWhenScenarioIsIgnored)
                                    )
                                .WithElse(ElseClause(Block(statementsWhenScenarioIsExecuted)));
            return testMethod.WithBody(testMethod.Body.AddStatements(ifElseStatement));
        }

        internal MethodDeclarationSyntax GenerateScenarioInitializeCall(StepsContainer scenario, MethodDeclarationSyntax testMethod)
        {
            var statement = _roslynHelper.AddSourceLinePragmaStatement(
                    IdentifierName("ScenarioInitialize"),
                    scenario.Location.Line
                    );
            var invocation = InvocationExpression(statement).WithArgumentList(_roslynHelper.GetArgumentList(IdentifierName("scenarioInfo")));

            return testMethod.WithBody(testMethod.Body.AddStatements(ExpressionStatement(invocation)));
        }

        internal ExpressionStatementSyntax GenerateScenarioCleanupMethodCall(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            // call scenario cleanup
            return ExpressionStatement(
                        InvocationExpression(
                            _roslynHelper.GetMemberAccess(ThisExpression(), GeneratorConstants.SCENARIO_CLEANUP_NAME)
                            )
                        );
            //testMethod.Body.AddStatements(
            //    ExpressionStatement(
            //        InvocationExpression(
            //            _roslynHelper.GetMemberAccess(ThisExpression(), GeneratorConstants.SCENARIO_CLEANUP_NAME)
            //            )
            //        )
            //    );
        }


        private InvocationExpressionSyntax CreateTestRunnerSkipScenarioCall()
        {
            return InvocationExpression(_roslynHelper.GetMemberAccess($"{TESTRUNNER_FIELD}.SkipScenario"));
        }

        private void GenerateScenarioOutlineExamplesAsIndividualMethods(
            ScenarioOutline scenarioOutline,
            TestClassGenerationContext generationContext,
            MethodDeclarationSyntax scenarioOutlineTestMethod, ParameterSubstitution paramToIdentifier)
        {
            var exampleSetIndex = 0;

            foreach (var exampleSet in scenarioOutline.Examples)
            {
                var useFirstColumnAsName = CanUseFirstColumnAsName(exampleSet.TableBody);
                string exampleSetIdentifier;

                if (string.IsNullOrEmpty(exampleSet.Name))
                {
                    if (scenarioOutline.Examples.Count(es => string.IsNullOrEmpty(es.Name)) > 1)
                    {
                        exampleSetIdentifier = $"ExampleSet {exampleSetIndex}".ToIdentifier();
                    }
                    else
                    {
                        exampleSetIdentifier = null;
                    }
                }
                else
                {
                    exampleSetIdentifier = exampleSet.Name.ToIdentifier();
                }


                foreach (var example in exampleSet.TableBody.Select((r, i) => new { Row = r, Index = i }))
                {
                    var variantName = useFirstColumnAsName ? example.Row.Cells.First().Value : string.Format("Variant {0}", example.Index);
                    GenerateScenarioOutlineTestVariant(generationContext, scenarioOutline, scenarioOutlineTestMethod, paramToIdentifier, exampleSet.Name ?? "", exampleSetIdentifier, example.Row, exampleSet.Tags, variantName);
                }

                exampleSetIndex++;
            }
        }

        //private void GenerateScenarioOutlineExamplesAsRowTests(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, CodeMemberMethod scenarioOutlineTestMethod)
        //{
        //    SetupTestMethod(generationContext, scenarioOutlineTestMethod, scenarioOutline, null, null, null, true);

        //    foreach (var examples in scenarioOutline.Examples)
        //    {
        //        foreach (var row in examples.TableBody)
        //        {
        //            var arguments = row.Cells.Select(c => c.Value);
        //            _unitTestGeneratorProvider.SetRow(generationContext, scenarioOutlineTestMethod, arguments, GetNonIgnoreTags(examples.Tags), HasIgnoreTag(examples.Tags));
        //        }
        //    }
        //}

        private ParameterSubstitution CreateParamToIdentifierMapping(ScenarioOutline scenarioOutline)
        {
            var paramToIdentifier = new ParameterSubstitution();
            foreach (var param in scenarioOutline.Examples.First().TableHeader.Cells)
            {
                paramToIdentifier.Add(param.Value, param.Value.ToIdentifierCamelCase());
            }

            //fix empty parameters
            var emptyStrings = paramToIdentifier.Where(kv => kv.Value == string.Empty).ToArray();
            foreach (var item in emptyStrings)
            {
                paramToIdentifier.Remove(item);
                paramToIdentifier.Add(item.Key, "_");
            }

            //fix duplicated parameter names
            for (int i = 0; i < paramToIdentifier.Count; i++)
            {
                int suffix = 1;
                while (paramToIdentifier.Take(i).Count(kv => kv.Value == paramToIdentifier[i].Value) > 0)
                {
                    paramToIdentifier[i] = new KeyValuePair<string, string>(paramToIdentifier[i].Key, paramToIdentifier[i].Value + suffix);
                    suffix++;
                }
            }


            return paramToIdentifier;
        }


        private bool CanUseFirstColumnAsName(IEnumerable<Gherkin.Ast.TableRow> tableBody)
        {
            if (tableBody.Any(r => !r.Cells.Any()))
            {
                return false;
            }

            return tableBody.Select(r => r.Cells.First().Value.ToIdentifier()).Distinct().Count() == tableBody.Count();
        }

        private MethodDeclarationSyntax CreateScenatioOutlineTestMethod(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, ParameterSubstitution paramToIdentifier)
        {
            var name = string.Format(GeneratorConstants.TEST_NAME_FORMAT, scenarioOutline.Name.ToIdentifier());
            var returnType = _roslynHelper.GetName("System.Collections.IEnumerator");

            List<SyntaxNodeOrToken> parameters = new();
            foreach (var pair in paramToIdentifier)
            {
                parameters.Add(Parameter(Identifier(pair.Value))
                                    .WithType(PredefinedType(Token(SyntaxKind.StringKeyword))));
                parameters.Add(Token(SyntaxKind.CommaToken));
            }
            parameters.Add(
                Parameter(Identifier(GeneratorConstants.SCENARIO_OUTLINE_EXAMPLE_TAGS_PARAMETER))
                    .WithType(
                        ArrayType(PredefinedType(Token(SyntaxKind.StringKeyword)))
                        .WithRankSpecifiers(SingletonList<ArrayRankSpecifierSyntax>(
                            ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression()))
                            )
                        )
                    )
                );

            var testMethod = _roslynHelper.CreateMethod(name, returnType)
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(parameters.ToArray())))
                .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)));
            return testMethod;
        }

        private void GenerateScenarioOutlineTestVariant(
            TestClassGenerationContext generationContext,
            ScenarioOutline scenarioOutline,
            MethodDeclarationSyntax scenarioOutlineTestMethod,
            IEnumerable<KeyValuePair<string, string>> paramToIdentifier,
            string exampleSetTitle,
            string exampleSetIdentifier,
            Gherkin.Ast.TableRow row,
            IEnumerable<Tag> exampleSetTags,
            string variantName)
        {
            var testMethod = CreateTestMethod(generationContext, scenarioOutline, exampleSetTags, variantName, exampleSetIdentifier);

            //call test implementation with the params

            var argumentList = row.Cells.Select(paramCell => _roslynHelper.StringLiteral(paramCell.Value)).ToList();


            argumentList.Add(_scenarioPartHelper.GetStringArrayExpression(exampleSetTags));


            var returnStatement = ReturnStatement(
                        InvocationExpression(_roslynHelper.GetMemberAccess(ThisExpression(), scenarioOutlineTestMethod.Identifier.Text))
                        .WithArgumentList(_roslynHelper.GetArgumentList(argumentList.ToArray()))
                        );
            returnStatement = _roslynHelper.AddSourceLinePragmaStatement(returnStatement, scenarioOutline.Location.Line);

            testMethod = testMethod.WithBody(Block(testMethod.Body.AddStatements(returnStatement)));

            generationContext.TestClass = generationContext.TestClass.AddMembers(testMethod);

            //_linePragmaHandler.AddLineDirectiveHidden(testMethod.Statements);
            var arguments = paramToIdentifier.Select((p2i, paramIndex) => new KeyValuePair<string, string>(p2i.Key, row.Cells.ElementAt(paramIndex).Value)).ToList();

            // Use the identifier of the example set (e.g. ExampleSet0, ExampleSet1) if we have it.
            // Otherwise, use the title of the example set provided by the user in the feature file.
            string exampleSetName = string.IsNullOrEmpty(exampleSetIdentifier) ? exampleSetTitle : exampleSetIdentifier;
            _unitTestGeneratorProvider.SetTestMethodAsRow(generationContext, testMethod, scenarioOutline.Name, exampleSetName, variantName, arguments);
        }

        private MethodDeclarationSyntax CreateTestMethod(
            TestClassGenerationContext generationContext,
            StepsContainer scenario,
            IEnumerable<Tag> additionalTags,
            string variantName = null,
            string exampleSetIdentifier = null)
        {
            var name = GetTestMethodName(scenario, variantName, exampleSetIdentifier);
            var testMethod = _roslynHelper.CreateMethod(name, _roslynHelper.GetName("System.Collections.IEnumerator"));
            testMethod = testMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            testMethod = SetupTestMethod(generationContext, testMethod, scenario, additionalTags, variantName, exampleSetIdentifier);

            return testMethod;
        }


        private MethodDeclarationSyntax SetupTestMethod(
            TestClassGenerationContext generationContext,
            MethodDeclarationSyntax testMethod,
            StepsContainer scenarioDefinition,
            IEnumerable<Tag> additionalTags,
            string variantName,
            string exampleSetIdentifier,
            bool rowTest = false)
        {
            var friendlyTestName = scenarioDefinition.Name;
            if (variantName != null)
            {
                friendlyTestName = $"{scenarioDefinition.Name}: {variantName}";
            }

            if (rowTest)
            {
                _unitTestGeneratorProvider.SetRowTest(generationContext, testMethod, friendlyTestName);
            }
            else
            {
                testMethod = _unitTestGeneratorProvider.MakeTestMethod(testMethod, friendlyTestName);
            }

            testMethod = _decoratorRegistry.DecorateTestMethod(generationContext, testMethod, ConcatTags(scenarioDefinition.GetTags(), additionalTags), out var scenarioCategories);

            if (scenarioCategories.Any())
            {
                testMethod = _unitTestGeneratorProvider.SetTestMethodCategories(testMethod, scenarioCategories);
            }

            return testMethod;
        }

        private static string GetTestMethodName(StepsContainer scenario, string variantName, string exampleSetIdentifier)
        {
            var methodName = string.Format(GeneratorConstants.TEST_NAME_FORMAT, scenario.Name.ToIdentifier());
            if (variantName == null)
            {
                return methodName;
            }

            var variantNameIdentifier = variantName.ToIdentifier().TrimStart('_');
            methodName = string.IsNullOrEmpty(exampleSetIdentifier)
                ? $"{methodName}_{variantNameIdentifier}"
                : $"{methodName}_{exampleSetIdentifier}_{variantNameIdentifier}";

            return methodName;
        }

        private IEnumerable<Tag> ConcatTags(params IEnumerable<Tag>[] tagLists)
        {
            return tagLists.Where(tagList => tagList != null).SelectMany(tagList => tagList);
        }
    }
}