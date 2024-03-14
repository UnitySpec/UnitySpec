using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace UnitySpec.Generator.Roslyn
{
    public class RoslynHelper
    {
        public ProviderLanguage TargetLanguage { get; private set; }

        public RoslynHelper(ProviderLanguage targetLanguage)
        {
            TargetLanguage = targetLanguage;
        }


        public SyntaxTrivia GetStartRegionStatement(string regionText)
        {
            return Trivia(
                    RegionDirectiveTrivia(true)
                    .WithEndOfDirectiveToken(
                        Token(
                            TriviaList(
                                PreprocessingMessage(regionText)
                                ),
                            SyntaxKind.EndOfDirectiveToken,
                            TriviaList()
                            )
                        )
                );
        }

        private SyntaxTrivia getPragmaToken(SyntaxKind keyword)
        {
            return Trivia(
                        PragmaWarningDirectiveTrivia(
                            Token(keyword),
                            true
                            )
                        );
        }
        public SyntaxTrivia GetDisableWarningsPragma()
        {
            return getPragmaToken(SyntaxKind.DisableKeyword);
        }

        public SyntaxTrivia GetEnableWarningsPragma()
        {
            return getPragmaToken(SyntaxKind.EnableKeyword);
        }

        private Version GetCurrentSpecFlowVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }

        public ClassDeclarationSyntax CreateGeneratedTypeDeclaration(string className)
        {
            return ClassDeclaration(className)
                .WithAttributeLists(
                    SingletonList<AttributeListSyntax>(
                        AttributeList(
                            SingletonSeparatedList<AttributeSyntax>(
                                Attribute(
                                    QualifiedName(
                                        QualifiedName(
                                            QualifiedName(
                                                IdentifierName("System"),
                                                IdentifierName("Runtime")
                                            ),
                                            IdentifierName("CompilerServices")
                                        ),
                                        IdentifierName("CompilerGeneratedAttribute")
                                    )
                                )
                                .WithArgumentList(
                                    AttributeArgumentList()
                                )
                            )
                        )
                    )
                )
                .WithModifiers(
                    TokenList(
                        new[]{
                            Token(SyntaxKind.PublicKeyword),
                            Token(SyntaxKind.PartialKeyword)
                        }
                    )
                );
        }

        public string GetErrorStatementString(string msg)
        {
            switch (TargetLanguage)
            {
                case ProviderLanguage.CSharp:
                    return "#error " + msg;
                default:
                    return msg;
            }
        }

        public AttributeListSyntax getAttribute(string attrType, string attrValue)
        {
            return
                AttributeList(
                    SingletonSeparatedList<AttributeSyntax>(
                        Attribute(GetName(attrType))
                        .WithArgumentList(
                            AttributeArgumentList(
                                SingletonSeparatedList(
                                    AttributeArgument(
                                        StringLiteral(attrValue)
                                        )
                                    )
                                )
                            )
                        )
                    )
                ;
        }

        public AttributeListSyntax getAttribute(string attrType)
        {
            return
                AttributeList(
                    SingletonSeparatedList<AttributeSyntax>(
                        Attribute(GetName(attrType))
                        .WithArgumentList(AttributeArgumentList())
                        )
                    )
                ;
        }

        public IEnumerable<AttributeListSyntax> getAttributeForEachValue(string attrType, IEnumerable<string> attrValues)
        {
            return attrValues.Select(value => getAttribute(attrType, value));
        }

        public MethodDeclarationSyntax CreateMethod(string methodName)
        {
            return CreateMethod(methodName, PredefinedType(Token(SyntaxKind.VoidKeyword)));
        }

        public MethodDeclarationSyntax CreateMethod(string methodName, TypeSyntax type)
        {
            var method = MethodDeclaration(type, Identifier(methodName)).WithBody(Block());
            return method;
        }


        public ReturnStatementSyntax AddSourceLinePragmaStatement(ReturnStatementSyntax returnStatement, int lineNo)
        {
            return returnStatement.WithReturnKeyword(
                                        Token(
                                            GetLineTrivia(lineNo),
                                            SyntaxKind.ReturnKeyword,
                                            TriviaList()
                                        )
                                    );
        }
        public MemberAccessExpressionSyntax AddSourceLinePragmaStatement(SimpleNameSyntax methodCall, int lineNo)
        {
            return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression()
                        .WithToken(
                            Token(
                                GetLineTrivia(lineNo),
                                SyntaxKind.ThisKeyword,
                                TriviaList()
                            )
                        ),
                        methodCall
                    );
        }

        public YieldStatementSyntax AddSourceLinePragmaStatement(YieldStatementSyntax methodCall, int lineNo)
        {
            return methodCall.WithYieldKeyword(
                Token(
                    GetLineTrivia(lineNo),
                    SyntaxKind.YieldKeyword,
                    TriviaList()
                    )
                );
        }

        public SyntaxTriviaList GetLineTrivia(int lineNo)
        {
            return TriviaList(
                        Trivia(
                            LineDirectiveTrivia(
                                Literal(lineNo),
                                true
                            )
                        )
                    );
        }

        internal NameSyntax GetName(string targetNamespace)
        {
            string[] splitString = targetNamespace.Split('.');
            System.Array.Reverse<string>(splitString);
            return GetNameSyntax(splitString);
        }

        internal UsingDirectiveSyntax GetUsing(string str)
        {
            return UsingDirective(GetName(str));
        }

        public ExpressionSyntax GetMemberAccess(string memberName)
        {
            string[] splitString = memberName.Split('.');
            System.Array.Reverse<string>(splitString);
            return GetMemberAccess(splitString);
        }
        public MemberAccessExpressionSyntax GetMemberAccess(ExpressionSyntax expressionSyntax, string memberName)
        {
            return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expressionSyntax,
                        IdentifierName(memberName)
                    );
        }
        public ExpressionSyntax GetMemberAccess(params string[] strings)
        {
            var len = strings.Length;
            if (len < 2)
            {
                return IdentifierName(strings[0]);
            }
            var result = MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName(strings[len - 1]),
                                IdentifierName(strings[len - 2])
                            );
            for (var i = len - 3; i >= 0; i--)
            {
                result = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, result, IdentifierName(strings[i]));
            }
            return result;
        }

        private NameSyntax GetNameSyntax(params string[] strings)
        {
            var len = strings.Length;
            if (len < 2) { return IdentifierName(strings[0]); }
            var result = QualifiedName(
                                IdentifierName(strings[len - 1]),
                                IdentifierName(strings[len - 2])
                            );
            for (var i = len - 3; i >= 0; i--)
            {
                result = QualifiedName(result, IdentifierName(strings[i]));
            }
            return result;
        }

        public ArgumentListSyntax GetArgumentList(ExpressionSyntax expr1)
        {
            return ArgumentList(
                        SingletonSeparatedList<ArgumentSyntax>(
                                Argument(expr1)
                        )
                    );
        }

        public ArgumentListSyntax GetArgumentList(params ExpressionSyntax[] expr)
        {
            var resultLen = expr.Length * 2 - 1;
            var resultList = new SyntaxNodeOrToken[resultLen];
            for (var i = 0; i < resultLen; i++)
            {
                if (i % 2 == 0)
                {
                    resultList[i] = Argument(expr[i / 2]);
                }
                else
                {
                    resultList[i] = Token(SyntaxKind.CommaToken);
                }
            }
            return ArgumentList(SeparatedList<ArgumentSyntax>(new SyntaxNodeOrTokenList(resultList)));
        }

        public SeparatedSyntaxList<ExpressionSyntax> GetInterspersedList(params ExpressionSyntax[] expr)
        {
            var resultLen = expr.Length * 2 - 1;
            var resultList = new SyntaxNodeOrToken[resultLen];
            for (var i = 0; i < resultLen; i++)
            {
                SyntaxNodeOrToken res;
                if (i % 2 == 0)
                {
                    res = expr[i / 2];
                }
                else
                {
                    res = Token(SyntaxKind.CommaToken);
                }
                resultList[i] = res;
            }

            return SeparatedList<ExpressionSyntax>(resultList);
        }

        public ExpressionSyntax StringLiteral(string value)
        {
            if (!(value is null))
            {
                return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
            }
            return LiteralExpression(SyntaxKind.NullLiteralExpression);
        }

        public ExpressionSyntax NumericLiteral(int value)
        {
            return LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(value));
        }

        public ArrayTypeSyntax StringArray(ExpressionSyntax size)
        {
            return Array(PredefinedType(Token(SyntaxKind.StringKeyword)), size);

        }

        public ArrayTypeSyntax Array(TypeSyntax type, ExpressionSyntax size)
        {
            return ArrayType(type)
                .WithRankSpecifiers(SingletonList<ArrayRankSpecifierSyntax>(
                    ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
                        size
                        ))
                    ));

        }

    }
}


