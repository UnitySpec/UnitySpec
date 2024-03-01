using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;


namespace UnityFlow.Generator.CodeDom
{
    public class CodeDomHelper
    {
        public CodeDomProviderLanguage TargetLanguage { get; private set; }

        //public CodeDomHelper(CodeDomProvider codeComProvider)
        //{
        //    switch (codeComProvider.FileExtension.ToLower(CultureInfo.InvariantCulture))
        //    {
        //        case "cs":
        //            TargetLanguage = CodeDomProviderLanguage.CSharp;
        //            break;
        //        case "vb":
        //            TargetLanguage = CodeDomProviderLanguage.VB;
        //            break;
        //        default:
        //            TargetLanguage = CodeDomProviderLanguage.Other;
        //            break;
        //    }
        //}

        public CodeDomHelper(CodeDomProviderLanguage targetLanguage)
        {
            TargetLanguage = targetLanguage;
        }

        //public CodeTypeReference CreateNestedTypeReference(CodeTypeDeclaration baseTypeDeclaration, string nestedTypeName)
        //{
        //    return new CodeTypeReference(baseTypeDeclaration.Name + "." + nestedTypeName);
        //}

        //private CodeStatement CreateCommentStatement(string comment)
        //{
        //    switch (TargetLanguage)
        //    {
        //        case CodeDomProviderLanguage.CSharp:
        //            return new CodeSnippetStatement("//" + comment);
        //        case CodeDomProviderLanguage.VB:
        //            return new CodeSnippetStatement("'" + comment);
        //    }

        //    throw TargetLanguageNotSupportedException();
        //}

        private NotImplementedException TargetLanguageNotSupportedException()
        {
            return new NotImplementedException($"{TargetLanguage} is not supported");
        }


        public void BindTypeToSourceFile(ClassDeclarationSyntax classDeclaration, string fileName)
        {
            switch (TargetLanguage)
            {
                case CodeDomProviderLanguage.VB:
                    //classDeclaration.Members.Add(new CodeSnippetTypeMember(string.Format("#ExternalSource(\"{0}\",1)", fileName)));
                    //classDeclaration.Members.Add(new CodeSnippetTypeMember("#End ExternalSource"));
                    break;

                case CodeDomProviderLanguage.CSharp:
                    classDeclaration.Members.First().Modifiers.Add(Token(
                                    TriviaList(
                                            new[]{
                                                Trivia(
                                                    LineDirectiveTrivia(
                                                        Literal(1),
                                                        true
                                                    )
                                                    .WithFile(
                                                        Literal("Move.feature")
                                                    )
                                                ),
                                                Trivia(
                                                    LineDirectiveTrivia(
                                                        Token(SyntaxKind.HiddenKeyword),
                                                        true
                                                    )
                                                )
                                            }
                                        ),
                                        SyntaxKind.PublicKeyword,
                                        TriviaList()
                                        ));
                    //classDeclaration.Members.Add(new CodeSnippetTypeMember(string.Format("#line 1 \"{0}\"", fileName)));
                    //classDeclaration.Members.Add(new CodeSnippetTypeMember("#line hidden"));
                    break;
            }
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

        public SyntaxTrivia GetEndRegionStatement()
        {
            return Trivia(
                    EndRegionDirectiveTrivia(true)
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
                case CodeDomProviderLanguage.CSharp:
                    return "#error " + msg;
                default:
                    return msg;
            }
        }

        public void AddAttribute(MemberDeclarationSyntax classDeclaration, string attrType, string attrValue)
        {
            classDeclaration.AddAttributeLists(
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
                );
        }

        public void AddAttribute(MemberDeclarationSyntax classDeclaration, string attrType)
        {
            classDeclaration.AddAttributeLists(
                AttributeList(
                    SingletonSeparatedList<AttributeSyntax>(
                        Attribute(GetName(attrType))
                        .WithArgumentList(AttributeArgumentList())
                        )
                    )
                );
        }

        public void AddAttributeForEachValue(MemberDeclarationSyntax codeTypeMember, string attrType, IEnumerable<string> attrValues)
        {
            foreach (var attrValue in attrValues)
                AddAttribute(codeTypeMember, attrType, attrValue);
        }

        //public CodeDomProvider CreateCodeDomProvider()
        //{
        //    switch (TargetLanguage)
        //    {
        //        case CodeDomProviderLanguage.CSharp:
        //            return new CSharpCodeProvider();
        //        case CodeDomProviderLanguage.VB:
        //            return new VBCodeProvider();
        //        default:
        //            throw new NotSupportedException();
        //    }
        //}

        public MethodDeclarationSyntax CreateMethod(ClassDeclarationSyntax outerClass, string methodName)
        {
            return CreateMethod(outerClass, methodName, PredefinedType(Token(SyntaxKind.VoidKeyword)));
        }

        public MethodDeclarationSyntax CreateMethod(ClassDeclarationSyntax outerClass, string methodName, TypeSyntax type)
        {
            var method = MethodDeclaration(type, Identifier(methodName));
            outerClass.Members.Add(method);
            return method;
        }

        //public CodeStatement CreateDisableSourceLinePragmaStatement()
        //{
        //    switch (TargetLanguage)
        //    {
        //        case CodeDomProviderLanguage.VB:
        //            return new CodeSnippetStatement("#End ExternalSource");
        //        case CodeDomProviderLanguage.CSharp:
        //            return new CodeSnippetStatement("#line hidden");
        //    }

        //    throw TargetLanguageNotSupportedException();
        //}


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
            var splitString = targetNamespace.Split('.');
            return GetNameSyntax(splitString);
        }

        internal UsingDirectiveSyntax GetUsing(string str)
        {
            return UsingDirective(GetName(str));
        }

        public MemberAccessExpressionSyntax GetMemberAccess(string memberName)
        {
            return GetMemberAccess(memberName.Split("."));
        }
        public MemberAccessExpressionSyntax GetMemberAccess(ExpressionSyntax expressionSyntax, string memberName)
        {
            return MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        expressionSyntax,
                        IdentifierName(memberName)
                    );
        }
        public MemberAccessExpressionSyntax GetMemberAccess(params string[] strings)
        {
            var len = strings.Length;
            if (len < 2) { throw new Exception("Member access for < 2"); }
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
            for (var i = 0; i < resultLen - 1; i ++) 
            {
                resultList[i] = i % 2 == 0 
                    ? Argument(expr[i / 2]) 
                    : Token(SyntaxKind.CommaToken);
            }

            return ArgumentList(
                        SeparatedList<ArgumentSyntax>(
                            resultList
                        )
                    );
        }

        public SeparatedSyntaxList<ExpressionSyntax> GetInterspersedList(params ExpressionSyntax[] expr)
        {
            var resultLen = expr.Length * 2 - 1;
            var resultList = new SyntaxNodeOrToken[resultLen];
            for (var i = 0; i < resultLen - 1; i++)
            {
                resultList[i] = i % 2 == 0
                    ? expr[i / 2]
                    : Token(SyntaxKind.CommaToken);
            }

            return SeparatedList<ExpressionSyntax>(resultList);
        }

        public ExpressionSyntax StringLiteral(string value)
        {
            return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
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
            return Array(type, size);
            //return ArrayType(type)
            //    .WithRankSpecifiers(SingletonList<ArrayRankSpecifierSyntax>(
            //        ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(
            //            size
            //            ))
            //        ));

        }

    }
}


