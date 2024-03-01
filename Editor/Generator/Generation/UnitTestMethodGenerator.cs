using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Gherkin.Ast;
using TechTalk.SpecFlow.Configuration;
using UnityFlow.Generator.CodeDom;
using UnityFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Parser;
using TechTalk.SpecFlow.Tracing;
using UnityFlow.Generator.UnitTestProvider;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis;

namespace UnityFlow.Generator.Generation
{
    public class UnitTestMethodGenerator
    {
        private const string IGNORE_TAG = "@Ignore";
        private const string TESTRUNNER_FIELD = "testRunner";
        private readonly CodeDomHelper _codeDomHelper;
        private readonly IDecoratorRegistry _decoratorRegistry;
        private readonly ScenarioPartHelper _scenarioPartHelper;
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly IUnitTestGeneratorProvider _unitTestGeneratorProvider;

        public UnitTestMethodGenerator(
            IUnitTestGeneratorProvider unitTestGeneratorProvider,
            IDecoratorRegistry decoratorRegistry,
            CodeDomHelper codeDomHelper,
            ScenarioPartHelper scenarioPartHelper,
            SpecFlowConfiguration specFlowConfiguration)
        {
            _unitTestGeneratorProvider = unitTestGeneratorProvider;
            _decoratorRegistry = decoratorRegistry;
            _codeDomHelper = codeDomHelper;
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

            GenerateTestBody(generationContext, scenarioOutline, scenarioOutlineTestMethod, feature, exampleTagsParam, paramToIdentifier);
        }

        private void GenerateTest(TestClassGenerationContext generationContext, Scenario scenario, SpecFlowFeature feature)
        {
            var testMethod = CreateTestMethod(generationContext, scenario, null);
            GenerateTestBody(generationContext, scenario, testMethod, feature);
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

        private void GenerateTestBody(
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

            AddVariableForTags(testMethod, tagsExpression);

            AddVariableForArguments(testMethod, paramToIdentifier);

            var arguments = _codeDomHelper.GetArgumentList(
                                _codeDomHelper.StringLiteral(scenario.Name),
                                _codeDomHelper.StringLiteral(scenario.Description),
                                _codeDomHelper.GetName(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME),
                                _codeDomHelper.GetName(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME),
                                _codeDomHelper.GetName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME)
                                );
            var scenarioInfoDecl = VariableDeclaration(_codeDomHelper.GetName("UnityFlow.ScenarioInfo"))
                                    .WithVariables(SingletonSeparatedList(
                                        VariableDeclarator(Identifier("scenarioInfo"))
                                        .WithInitializer(EqualsValueClause(
                                            ObjectCreationExpression(_codeDomHelper.GetName("UnitFlow.ScenarioInfo"))
                                            .WithArgumentList(arguments)
                                        ))
                                    ));
            testMethod.Body.AddStatements(LocalDeclarationStatement(scenarioInfoDecl));

            GenerateScenarioInitializeCall(generationContext, scenario, testMethod);

            GenerateTestMethodBody(generationContext, scenario, testMethod, paramToIdentifier, feature);

            GenerateScenarioCleanupMethodCall(generationContext, testMethod);
        }

        private void AddVariableForTags(MethodDeclarationSyntax testMethod, ExpressionSyntax tagsExpression)
        {
            var tagVariable = VariableDeclaration(_codeDomHelper.StringArray(OmittedArraySizeExpression()))
                .WithVariables(SingletonSeparatedList(VariableDeclarator(Identifier(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME))
                    .WithInitializer(EqualsValueClause(tagsExpression))
                    ));

            testMethod.Body.Statements.Add(LocalDeclarationStatement(tagVariable));
        }

        private void AddVariableForArguments(MethodDeclarationSyntax testMethod, ParameterSubstitution paramToIdentifier)
        {
            var type = _codeDomHelper.GetName("System.Collections.Specialized.OrderedDictionary");
            var variableDecl = VariableDeclaration(type)
                .WithVariables(SingletonSeparatedList(
                    VariableDeclarator(Identifier(GeneratorConstants.SCENARIO_ARGUMENTS_VARIABLE_NAME))
                    .WithInitializer(EqualsValueClause(
                        ObjectCreationExpression(type)
                        .WithArgumentList(ArgumentList())
                        ))
                    ));

            testMethod.Body.Statements.Add(LocalDeclarationStatement(variableDecl));

            if (paramToIdentifier != null)
            {
                foreach (var parameter in paramToIdentifier)
                {
                    var addArgumentExpression = ExpressionStatement(
                        InvocationExpression(_codeDomHelper.GetMemberAccess("argumentsOfScenario.Add"))
                        .WithArgumentList(_codeDomHelper.GetArgumentList(
                            _codeDomHelper.StringLiteral(parameter.Key),
                            IdentifierName(parameter.Value)
                            )
                        ));

                    testMethod.Body.Statements.Add(addArgumentExpression);
                }
            }
        }

        internal void GenerateTestMethodBody(TestClassGenerationContext generationContext, StepsContainer scenario, MethodDeclarationSyntax testMethod, ParameterSubstitution paramToIdentifier, SpecFlowFeature feature)
        {
            var statementsWhenScenarioIsIgnored = SingletonList(ExpressionStatement(CreateTestRunnerSkipScenarioCall()));
            var statementsWhenScenarioIsExecuted = new List<StatementSyntax>
            {
                ExpressionStatement(InvocationExpression(_codeDomHelper.GetMemberAccess(ThisExpression(), "ScenarioStart")))
            };


            if (generationContext.Feature.HasFeatureBackground())
            {
                var thisExpr = _codeDomHelper.AddSourceLinePragmaStatement(IdentifierName(GeneratorConstants.BACKGROUND_NAME), scenario.Location.Line);
                statementsWhenScenarioIsExecuted.Add(ExpressionStatement(InvocationExpression(thisExpr)));
            }


            foreach (var scenarioStep in scenario.Steps)
            {
                _scenarioPartHelper.GenerateStep(generationContext, statementsWhenScenarioIsExecuted, scenarioStep, paramToIdentifier);
            }

            var tagsOfScenarioVariableReferenceExpression = IdentifierName(GeneratorConstants.SCENARIO_TAGS_VARIABLE_NAME);
            var featureFileTagFieldReferenceExpression = IdentifierName(GeneratorConstants.FEATURE_TAGS_VARIABLE_NAME);

            var tagHelperReference = IdentifierName(nameof(TagHelper));
            var scenarioTagIgnoredCheckStatement = InvocationExpression(_codeDomHelper.GetMemberAccess(tagHelperReference, nameof(TagHelper.ContainsIgnoreTag)))
                                                    .WithArgumentList(_codeDomHelper.GetArgumentList(tagsOfScenarioVariableReferenceExpression));
            var featureTagIgnoredCheckStatement = InvocationExpression(_codeDomHelper.GetMemberAccess(tagHelperReference, nameof(TagHelper.ContainsIgnoreTag)))
                                                    .WithArgumentList(_codeDomHelper.GetArgumentList(featureFileTagFieldReferenceExpression));
            var conditionStatement = BinaryExpression(SyntaxKind.LogicalOrExpression, scenarioTagIgnoredCheckStatement, featureTagIgnoredCheckStatement);
            var ifElseStatement = IfStatement(
                                    ParenthesizedExpression(conditionStatement),
                                    Block(statementsWhenScenarioIsIgnored)
                                    )
                                .WithElse(ElseClause(Block(statementsWhenScenarioIsExecuted)));

            testMethod.Body.AddStatements(ifElseStatement);
        }

        internal void GenerateScenarioInitializeCall(TestClassGenerationContext generationContext, StepsContainer scenario, MethodDeclarationSyntax testMethod)
        {
            var statement = _codeDomHelper.AddSourceLinePragmaStatement(
                    IdentifierName("ScenarioInitialize"), 
                    scenario.Location.Line
                    );
            var invocation = InvocationExpression(statement).WithArgumentList(_codeDomHelper.GetArgumentList(IdentifierName("scenarioInfo")));

            testMethod.Body.Statements.Add(ExpressionStatement(invocation));
        }

        internal void GenerateScenarioCleanupMethodCall(TestClassGenerationContext generationContext, MethodDeclarationSyntax testMethod)
        {
            // call scenario cleanup
            testMethod.Body.AddStatements(
                ExpressionStatement(
                    InvocationExpression(
                        _codeDomHelper.GetMemberAccess(ThisExpression(), GeneratorConstants.SCENARIO_CLEANUP_NAME)
                        )
                    )
                );
        }


        private InvocationExpressionSyntax CreateTestRunnerSkipScenarioCall()
        {
            return InvocationExpression(_codeDomHelper.GetMemberAccess($"{TESTRUNNER_FIELD}.SkipScenario"));
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

        //private void GenerateScenarioOutlineExamplesAsRowTests(TestClassGenerationContext generationContext, ScenarioOutline scenarioOutline, CodeMemberMethod scenatioOutlineTestMethod)
        //{
        //    SetupTestMethod(generationContext, scenatioOutlineTestMethod, scenarioOutline, null, null, null, true);

        //    foreach (var examples in scenarioOutline.Examples)
        //    {
        //        foreach (var row in examples.TableBody)
        //        {
        //            var arguments = row.Cells.Select(c => c.Value);
        //            _unitTestGeneratorProvider.SetRow(generationContext, scenatioOutlineTestMethod, arguments, GetNonIgnoreTags(examples.Tags), HasIgnoreTag(examples.Tags));
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
            var returnType = _codeDomHelper.GetName("System.Collections.IEnumerator");

            var parameters = new SyntaxNodeOrTokenList();
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

            var testMethod = _codeDomHelper.CreateMethod(generationContext.TestClass, name, returnType)
                .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(parameters.ToArray())));

            testMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            return testMethod;
        }

        private void GenerateScenarioOutlineTestVariant(
            TestClassGenerationContext generationContext,
            ScenarioOutline scenarioOutline,
            MethodDeclarationSyntax scenatioOutlineTestMethod,
            IEnumerable<KeyValuePair<string, string>> paramToIdentifier,
            string exampleSetTitle,
            string exampleSetIdentifier,
            Gherkin.Ast.TableRow row,
            IEnumerable<Tag> exampleSetTags,
            string variantName)
        {
            var testMethod = CreateTestMethod(generationContext, scenarioOutline, exampleSetTags, variantName, exampleSetIdentifier);


            //call test implementation with the params
            
            var argumentList = row.Cells.Select(paramCell => _codeDomHelper.StringLiteral(paramCell.Value)).ToList();
            

            argumentList.Add(_scenarioPartHelper.GetStringArrayExpression(exampleSetTags));


            var returnStatement = ReturnStatement(
                        InvocationExpression(_codeDomHelper.GetMemberAccess(ThisExpression(), "BasicMovement"))
                        .WithArgumentList(_codeDomHelper.GetArgumentList(argumentList.ToArray()))
                        );
            returnStatement = _codeDomHelper.AddSourceLinePragmaStatement( returnStatement, scenarioOutline.Location.Line );

            testMethod.Body.Statements.Add(returnStatement);

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
            var testMethod = _codeDomHelper.CreateMethod(generationContext.TestClass, name, _codeDomHelper.GetName("System.Collections.IEnumerator"));
            testMethod.AddModifiers(Token(SyntaxKind.PublicKeyword));

            SetupTestMethod(generationContext, testMethod, scenario, additionalTags, variantName, exampleSetIdentifier);

            return testMethod;
        }


        private void SetupTestMethod(
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
                _unitTestGeneratorProvider.SetTestMethod(generationContext, testMethod, friendlyTestName);
            }

            _decoratorRegistry.DecorateTestMethod(generationContext, testMethod, ConcatTags(scenarioDefinition.GetTags(), additionalTags), out var scenarioCategories);

            if (scenarioCategories.Any())
            {
                _unitTestGeneratorProvider.SetTestMethodCategories(generationContext, testMethod, scenarioCategories);
            }
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