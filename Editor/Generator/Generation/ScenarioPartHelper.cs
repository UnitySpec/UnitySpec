using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Gherkin.Ast;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using UnityFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Parser;
using UnityFlow;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using Microsoft.CodeAnalysis.CSharp;
using System;

namespace UnityFlow.Generator.Generation
{

    public class ScenarioPartHelper
    {
        private readonly SpecFlowConfiguration _specFlowConfiguration;
        private readonly CodeDomHelper _codeDomHelper;
        private int _tableCounter;


        public ScenarioPartHelper(SpecFlowConfiguration specFlowConfiguration, CodeDomHelper codeDomHelper)
        {
            _specFlowConfiguration = specFlowConfiguration;
            _codeDomHelper = codeDomHelper;
        }

        public void SetupFeatureBackground(TestClassGenerationContext generationContext)
        {
            if (!generationContext.Feature.HasFeatureBackground())
            {
                return;
            }

            var background = generationContext.Feature.Background;

            var backgroundMethod = generationContext.FeatureBackgroundMethod;

            backgroundMethod.Modifiers.Add(Token(SyntaxKind.PublicKeyword));


            var statements = new List<StatementSyntax>();
            //using (new SourceLineScope(_specFlowConfiguration, _codeDomHelper, statements, generationContext.Document.SourceFilePath, background.Location))
            //{
            //}

            foreach (var step in background.Steps)
            {
                GenerateStep(generationContext, statements, step, null);
            }
            backgroundMethod.Body.Statements.AddRange(statements);

        }

        public void GenerateStep(TestClassGenerationContext generationContext, List<StatementSyntax> statements, Step gherkinStep, ParameterSubstitution paramToIdentifier)
        {
            var testRunnerField = GetTestRunnerExpression();
            var scenarioStep = AsSpecFlowStep(gherkinStep);

            var argumentList = _codeDomHelper.GetArgumentList(
                GetSubstitutedString(scenarioStep.Text, paramToIdentifier),
                GetDocStringArgExpression(scenarioStep.Argument as DocString, paramToIdentifier),
                GetTableArgExpression(scenarioStep.Argument as DataTable, statements, paramToIdentifier),
                _codeDomHelper.StringLiteral(scenarioStep.Keyword)
                );

            var invokeExpr = ParenthesizedExpression(CastExpression(
                _codeDomHelper.GetName("System.Collections.IEnumerator"),
                ParenthesizedExpression(InvocationExpression(
                        _codeDomHelper.GetMemberAccess($"{GeneratorConstants.TESTRUNNER_FIELD}.{scenarioStep.StepKeyword}")
                    ).WithArgumentList(argumentList)
                )));

            var yieldStmt = _codeDomHelper.AddSourceLinePragmaStatement(YieldStatement(SyntaxKind.YieldReturnStatement, invokeExpr), gherkinStep.Location.Line);
            //testRunner.Given("something");
            statements.Add(yieldStmt);
        }

        public ExpressionSyntax GetStringArrayExpression(IEnumerable<Tag> tags)
        {
            if (!tags.Any())
            {
                return ParenthesizedExpression(CastExpression(
                            _codeDomHelper.StringArray(OmittedArraySizeExpression()),
                            ParenthesizedExpression(LiteralExpression(SyntaxKind.NullLiteralExpression))
                            ));
            }

            var tagExprs = tags.Select(tag => _codeDomHelper.StringLiteral(tag.GetNameWithoutAt())).ToArray();
            return ParenthesizedExpression(
                ArrayCreationExpression(
                    _codeDomHelper.StringArray(_codeDomHelper.NumericLiteral(tagExprs.Length))
                    )
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ArrayInitializerExpression,
                        _codeDomHelper.GetInterspersedList( tagExprs )
                        )
                    )
                );

        }



        private SpecFlowStep AsSpecFlowStep(Step step)
        {
            var specFlowStep = step as SpecFlowStep;
            if (specFlowStep == null)
            {
                throw new TestGeneratorException("The step must be a SpecFlowStep.");
            }

            return specFlowStep;
        }

        private ExpressionSyntax GetTableArgExpression(DataTable tableArg, List<StatementSyntax> statements, ParameterSubstitution paramToIdentifier)
        {
            throw new NotImplementedException("Table expressions not yet implemented in Unity");
            //var tableType = _codeDomHelper.GetName("UnityFlow.Table");
            //if (tableArg == null)
            //{
            //    return CastExpression(tableType, LiteralExpression(SyntaxKind.NullLiteralExpression));
            //}

            //_tableCounter++;

            ////TODO[Gherkin3]: remove dependency on having the first row as header
            //var header = tableArg.Rows.First();
            //var body = tableArg.Rows.Skip(1).ToArray();

            ////Table table0 = new Table(header...);
            //var tableVar = IdentifierName("table" + _tableCounter);
            //statements.Add(
            //    new CodeVariableDeclarationStatement(typeof(Table), tableVar.VariableName,
            //        new CodeObjectCreateExpression(
            //            typeof(Table),
            //            GetStringArrayExpression(header.Cells.Select(c => c.Value), paramToIdentifier))));

            //foreach (var row in body)
            //{
            //    //table0.AddRow(cells...);
            //    statements.Add(new CodeExpressionStatement(
            //        new CodeMethodInvokeExpression(
            //            tableVar,
            //            "AddRow",
            //            GetStringArrayExpression(row.Cells.Select(c => c.Value), paramToIdentifier))));
            //}

            //return tableVar;
        }

        private ExpressionSyntax GetDocStringArgExpression(DocString docString, ParameterSubstitution paramToIdentifier)
        {
            return GetSubstitutedString(docString == null ? null : docString.Content, paramToIdentifier);
        }

        public NameSyntax GetTestRunnerExpression()
        {
            return IdentifierName(GeneratorConstants.TESTRUNNER_FIELD);
        }

        //private ExpressionSyntax GetStringArrayExpression(IEnumerable<string> items, ParameterSubstitution paramToIdentifier)
        //{
        //    return new CodeArrayCreateExpression(typeof(string[]), items.Select(item => GetSubstitutedString(item, paramToIdentifier)).ToArray());
        //}

        private ExpressionSyntax GetSubstitutedString(string text, ParameterSubstitution paramToIdentifier)
        {
            if (text == null)
            {
                return CastExpression(
                        PredefinedType(
                            Token(SyntaxKind.StringKeyword)
                        ),
                        ParenthesizedExpression(
                            LiteralExpression(
                                SyntaxKind.NullLiteralExpression
                            )
                        )
                    );
            }

            if (paramToIdentifier == null)
            {
                return _codeDomHelper.StringLiteral(text);
            }

            var paramRe = new Regex(@"\<(?<param>[^\<\>]+)\>");
            var formatText = text.Replace("{", "{{").Replace("}", "}}");
            var arguments = new List<string>();

            formatText = paramRe.Replace(formatText, match =>
            {
                var param = match.Groups["param"].Value;
                string id;
                if (!paramToIdentifier.TryGetIdentifier(param, out id))
                {
                    return match.Value;
                }

                var argIndex = arguments.IndexOf(id);
                if (argIndex < 0)
                {
                    argIndex = arguments.Count;
                    arguments.Add(id);
                }

                return "{" + argIndex + "}";
            });

            if (arguments.Count == 0)
            {
                return _codeDomHelper.StringLiteral(text);
            }

            ExpressionSyntax[] args = arguments.Select(id => IdentifierName(id)).Prepend(_codeDomHelper.StringLiteral(formatText)).ToArray();
            var formatArguments = _codeDomHelper.GetArgumentList(args);

            return InvocationExpression(_codeDomHelper.GetName("String.Format"), formatArguments);
        }
    }

}