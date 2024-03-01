using BoDi;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.Interfaces;
using UnityFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;
using TechTalk.SpecFlow.Tracing;
using UnityFlow.Generator.CodeDom;
using UnityFlow.Generator.Generation;

namespace UnityFlow.Generator
{
    public class UnityTestGenerator : ErrorHandlingTestGenerator, ITestGenerator
    {
        protected readonly SpecFlowConfiguration specFlowConfiguration;
        protected readonly ProjectSettings projectSettings;
        protected readonly ITestHeaderWriter testHeaderWriter;
        protected readonly ITestUpToDateChecker testUpToDateChecker;
        protected readonly CodeDomHelper codeDomHelper;
        private readonly IFeatureGeneratorRegistry featureGeneratorRegistry;
        private readonly IGherkinParserFactory gherkinParserFactory;


        public UnityTestGenerator(SpecFlowConfiguration specFlowConfiguration,
            ProjectSettings projectSettings,
            ITestHeaderWriter testHeaderWriter,
            ITestUpToDateChecker testUpToDateChecker,
            IFeatureGeneratorRegistry featureGeneratorRegistry,
            CodeDomHelper codeDomHelper,
            IGherkinParserFactory gherkinParserFactory)
        {
            if (specFlowConfiguration == null) throw new ArgumentNullException(nameof(specFlowConfiguration));
            if (projectSettings == null) throw new ArgumentNullException(nameof(projectSettings));
            if (testHeaderWriter == null) throw new ArgumentNullException(nameof(testHeaderWriter));
            if (testUpToDateChecker == null) throw new ArgumentNullException(nameof(testUpToDateChecker));
            if (featureGeneratorRegistry == null) throw new ArgumentNullException(nameof(featureGeneratorRegistry));
            if (gherkinParserFactory == null) throw new ArgumentNullException(nameof(gherkinParserFactory));

            this.specFlowConfiguration = specFlowConfiguration;
            this.testUpToDateChecker = testUpToDateChecker;
            this.featureGeneratorRegistry = featureGeneratorRegistry;
            this.codeDomHelper = codeDomHelper;
            this.testHeaderWriter = testHeaderWriter;
            this.projectSettings = projectSettings;
            this.gherkinParserFactory = gherkinParserFactory;
        }

        protected override TestGeneratorResult GenerateTestFileWithExceptions(FeatureFileInput featureFileInput, GenerationSettings settings)
        {
            if (featureFileInput == null) throw new ArgumentNullException("featureFileInput");
            if (settings == null) throw new ArgumentNullException("settings");

            var generatedTestFullPath = GetTestFullPath(featureFileInput);
            bool? preliminaryUpToDateCheckResult = null;
            if (settings.CheckUpToDate)
            {
                preliminaryUpToDateCheckResult = testUpToDateChecker.IsUpToDatePreliminary(featureFileInput, generatedTestFullPath, settings.UpToDateCheckingMethod);
                if (preliminaryUpToDateCheckResult == true)
                    return new TestGeneratorResult(null, true);
            }

            string generatedTestCode = GetGeneratedTestCode(featureFileInput);
            if (string.IsNullOrEmpty(generatedTestCode))
                return new TestGeneratorResult(null, true);

            if (settings.CheckUpToDate && preliminaryUpToDateCheckResult != false)
            {
                var isUpToDate = testUpToDateChecker.IsUpToDate(featureFileInput, generatedTestFullPath, generatedTestCode, settings.UpToDateCheckingMethod);
                if (isUpToDate)
                    return new TestGeneratorResult(null, true);
            }

            if (settings.WriteResultToFile)
            {
                File.WriteAllText(generatedTestFullPath, generatedTestCode, Encoding.UTF8);
            }

            return new TestGeneratorResult(generatedTestCode, false);
        }

        protected string GetGeneratedTestCode(FeatureFileInput featureFileInput)
        {
            var classDeclaration = GenerateTestFileCode(featureFileInput);
            var header = GetSpecFlowHeader();
            var footer = GetSpecFlowFooter();

            CompilationUnitSyntax unit =
                SyntaxFactory.CompilationUnit()
                .WithMembers(
                    SyntaxFactory.SingletonList<MemberDeclarationSyntax>(
                        classDeclaration
                        .WithNamespaceKeyword(header)
                        )
                    )
                .WithEndOfFileToken(footer)
                .NormalizeWhitespace()
                ;

            return unit.ToFullString();
            //using (var outputWriter = new IndentProcessingWriter(new StringWriter()))
            //{
            //    var codeProvider = codeDomHelper.CreateCodeDomProvider();
            //    var codeNamespace = GenerateTestFileCode(featureFileInput);

            //    if (codeNamespace == null) return "";

            //    var options = new CodeGeneratorOptions
            //    {
            //        BracingStyle = "C",
            //    };

            //    AddSpecFlowHeader(codeProvider, outputWriter);
            //    codeProvider.GenerateCodeFromNamespace(codeNamespace, outputWriter, options);
            //    AddSpecFlowFooter(codeProvider, outputWriter);

            //    outputWriter.Flush();
            //    var generatedTestCode = outputWriter.ToString();
            //    return FixVBNetGlobalNamespace(generatedTestCode);
            //}
        }

        private string FixVBNetGlobalNamespace(string generatedTestCode)
        {
            return generatedTestCode
                    .Replace("Global.GlobalVBNetNamespace", "Global")
                    .Replace("GlobalVBNetNamespace", "Global");
        }

        private NamespaceDeclarationSyntax GenerateTestFileCode(FeatureFileInput featureFileInput)
        {
            string targetNamespace = GetTargetNamespace(featureFileInput) ?? "SpecFlow.GeneratedTests";

            var parser = gherkinParserFactory.Create(specFlowConfiguration.FeatureLanguage);
            SpecFlowDocument specFlowDocument;
            using (var contentReader = featureFileInput.GetFeatureFileContentReader(projectSettings))
            {
                specFlowDocument = ParseContent(parser, contentReader, GetSpecFlowDocumentLocation(featureFileInput));
            }

            if (specFlowDocument.SpecFlowFeature == null) return null;

            var featureGenerator = featureGeneratorRegistry.CreateGenerator(specFlowDocument);

            var codeNamespace = featureGenerator.GenerateUnitTestFixture(specFlowDocument, null, targetNamespace);
            return codeNamespace;
        }

        private SpecFlowDocumentLocation GetSpecFlowDocumentLocation(FeatureFileInput featureFileInput)
        {
            return new SpecFlowDocumentLocation(
                featureFileInput.GetFullPath(projectSettings),
                GetFeatureFolderPath(featureFileInput.ProjectRelativePath));
        }

        private string GetFeatureFolderPath(string projectRelativeFilePath)
        {
            string directoryName = Path.GetDirectoryName(projectRelativeFilePath);
            if (string.IsNullOrWhiteSpace(directoryName)) return null;

            return string.Join("/", directoryName.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries));
        }

        protected virtual SpecFlowDocument ParseContent(IGherkinParser parser, TextReader contentReader, SpecFlowDocumentLocation documentLocation)
        {
            return parser.Parse(contentReader, documentLocation);
        }

        protected string GetTargetNamespace(FeatureFileInput featureFileInput)
        {
            if (!string.IsNullOrEmpty(featureFileInput.CustomNamespace))
                return featureFileInput.CustomNamespace;

            string targetNamespace = projectSettings == null || string.IsNullOrEmpty(projectSettings.DefaultNamespace)
                ? null
                : projectSettings.DefaultNamespace;

            var directoryName = Path.GetDirectoryName(featureFileInput.ProjectRelativePath);
            string namespaceExtension = string.IsNullOrEmpty(directoryName) ? null :
                string.Join(".", directoryName.TrimStart('\\', '/', '.').Split('\\', '/').Select(f => f.ToIdentifier()).ToArray());
            if (!string.IsNullOrEmpty(namespaceExtension))
                targetNamespace = targetNamespace == null
                    ? namespaceExtension
                    : targetNamespace + "." + namespaceExtension;
            return targetNamespace;
        }

        public string GetTestFullPath(FeatureFileInput featureFileInput)
        {
            var path = featureFileInput.GetGeneratedTestFullPath(projectSettings);
            if (path != null)
                return path;

            return featureFileInput.GetFullPath(projectSettings) + GenerationTargetLanguage.GetExtension(projectSettings.ProjectPlatformSettings.Language);
        }

        #region Header & Footer
        protected override Version DetectGeneratedTestVersionWithExceptions(FeatureFileInput featureFileInput)
        {
            var generatedTestFullPath = GetTestFullPath(featureFileInput);
            return testHeaderWriter.DetectGeneratedTestVersion(featureFileInput.GetGeneratedTestContent(generatedTestFullPath));
        }

        protected SyntaxToken GetSpecFlowHeader()
        {
            var version = GetCurrentSpecFlowVersion();

            return SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(
                            new[]{
                                SyntaxFactory.Comment("// ------------------------------------------------------------------------------"),
                                SyntaxFactory.Comment("//  <auto-generated>"),
                                SyntaxFactory.Comment("//      This code was generated by UnityFlow (https://github.com/mmulder135/UnityFlow)."),
                                SyntaxFactory.Comment($"//      UnityFlow version: {version}"),
                                SyntaxFactory.Comment("// "),
                                SyntaxFactory.Comment("//      Changes to this file may cause incorrect behavior and will be lost if"),
                                SyntaxFactory.Comment("//      the code is regenerated."),
                                SyntaxFactory.Comment("//  </auto-generated>"),
                                SyntaxFactory.Comment("// ------------------------------------------------------------------------------"),

                                codeDomHelper.GetStartRegionStatement("Designer generated code"),
                                codeDomHelper.GetDisableWarningsPragma()

                            }
                        ),
                        SyntaxKind.NamespaceKeyword,
                        SyntaxFactory.TriviaList()
                );
        }
        //        protected void AddSpecFlowHeader(CodeDomProvider codeProvider, TextWriter outputWriter)
        //        {
        //            const string specFlowHeaderTemplate = @"------------------------------------------------------------------------------
        // <auto-generated>
        //     This code was generated by UnityFlow (https://github.com/mmulder135/UnityFlow).
        //     UnityFlow version: {0}

        //     Changes to this file may cause incorrect behavior and will be lost if
        //     the code is regenerated.
        // </auto-generated>
        //------------------------------------------------------------------------------";

        //            var headerReader = new StringReader(string.Format(specFlowHeaderTemplate,
        //                GetCurrentSpecFlowVersion()
        //                ));

        //            string line;
        //            while ((line = headerReader.ReadLine()) != null)
        //            {
        //                codeProvider.GenerateCodeFromStatement(new CodeCommentStatement(line), outputWriter, null);
        //            }

        //            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetStartRegionStatement("Designer generated code"), outputWriter, null);
        //            codeProvider.GenerateCodeFromStatement(codeDomHelper.GetDisableWarningsPragma(), outputWriter, null);
        //        }

        protected SyntaxToken GetSpecFlowFooter()
        {
            return SyntaxFactory.Token(
                        SyntaxFactory.TriviaList(
                            new[]{
                                codeDomHelper.GetEnableWarningsPragma(),
                                codeDomHelper.GetEndRegionStatement()
                            }
                        ),
                        SyntaxKind.EndOfFileToken,
                        SyntaxFactory.TriviaList()
                    );
        }
        //protected void AddSpecFlowFooter(CodeDomProvider codeProvider, TextWriter outputWriter)
        //{
        //    codeProvider.GenerateCodeFromStatement(codeDomHelper.GetEnableWarningsPragma(), outputWriter, null);
        //    codeProvider.GenerateCodeFromStatement(codeDomHelper.GetEndRegionStatement(), outputWriter, null);
        //}

        protected Version GetCurrentSpecFlowVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version;
        }
        #endregion
    }
}
