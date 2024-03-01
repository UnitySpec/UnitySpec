using System.IO;
using System.Linq;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using UnityFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Generator.Configuration;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.Project;
using UnityFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Parser;
using UnityEngine;

namespace UnityFlow.Generator
{
    public class Compiler
    {
        public string TestClassNameFormat { get; set; } = "{0}Feature";

        private CodeDomHelper _codeDomHelper;
        private string _specsPath;
        private string _projectFilePath;
        private string _projectFolder;
        private string _outputPath;
        private string _rootNamespace;

        public Compiler()
        {
            _projectFolder = Path.GetFullPath(Application.dataPath);
            _projectFilePath = Path.Combine(_projectFolder, "..\\Specs.csproj"); // TODO: Get from config
            _outputPath = Path.Combine(_projectFolder, "output");
            _specsPath = Path.Combine(_projectFolder, "Specs");
            _codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);
            _rootNamespace = "";
        }
        public void Generate()
        {
            string[] files = Directory.GetFiles(_specsPath, "*.feature", SearchOption.AllDirectories);

            ConfigurationLoader configurationLoader = new ConfigurationLoader(new SpecFlowJsonLocator());
            ProjectReader reader = new ProjectReader(new GeneratorConfigurationProvider(configurationLoader),
                                           new ProjectLanguageReader());

            SpecFlowProject specFlowProject = reader.ReadSpecFlowProject(_projectFilePath, _rootNamespace);
            TestHeaderWriter testHeaderWriter = new TestHeaderWriter();
            var objectContainer = new GeneratorContainerBuilder().CreateContainer(
                new SpecFlowConfigurationHolder(ConfigSource.Default, null),
                new ProjectSettings(),
                Enumerable.Empty<GeneratorPluginInfo>()
                );
            ITestGenerator testGenerator = new UnityTestGenerator(
                ConfigurationLoader.GetDefault(), // TODO: own config
                specFlowProject.ProjectSettings,
                testHeaderWriter,
                new TestUpToDateChecker(testHeaderWriter, new GeneratorInfo(), specFlowProject.ProjectSettings),
                new FeatureGeneratorRegistry(objectContainer),
                _codeDomHelper,
                new SpecFlowGherkinParserFactory()
                );
            UnityFeatureCodeBehindGenerator featureCodeBehindGenerator = new UnityFeatureCodeBehindGenerator(testGenerator);
            UnityFeatureFileCodeBehindGenerator codeGenerator = new UnityFeatureFileCodeBehindGenerator(featureCodeBehindGenerator);
            var res = codeGenerator.GenerateFilesForProject(files, _projectFolder, _outputPath);
            foreach (string path in res)
            {
                Debug.Log($"Generated file in {path}");
            }
        }
    }
}