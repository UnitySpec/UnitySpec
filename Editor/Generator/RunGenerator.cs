using System.Collections.Generic;
using UnityFlow.General.Build;
using UnityFlow.General.Configuration;
using UnityFlow.General.GeneratorInterfaces;
using UnityFlow.General.Utils;
using UnityFlow.Generator.Configuration;
using UnityFlow.Generator.Project;

namespace UnityFlow.Generator
{
    public class RunGenerator
    {
        private readonly FilePathGenerator _filePathGenerator;
        private readonly FeatureCodeBehindGenerator _featureCodeBehindGenerator;
        private readonly ITaskLoggingWrapper _loggingWrapper;

        public RunGenerator(string projectFilePath, string rootNamespace, ITaskLoggingWrapper log)
        {
            _filePathGenerator = new FilePathGenerator();

            ConfigurationLoader configurationLoader = new ConfigurationLoader(new SpecFlowJsonLocator());
            ProjectReader reader = new ProjectReader(new GeneratorConfigurationProvider(configurationLoader),
                                           new ProjectLanguageReader());

            SpecFlowProject specFlowProject = reader.ReadSpecFlowProject(projectFilePath, rootNamespace);

            ITestGenerator testGen = new TestGeneratorFactory().CreateGenerator(specFlowProject.ProjectSettings, new GeneratorPluginInfo[0]);
            _featureCodeBehindGenerator = new FeatureCodeBehindGenerator(testGen);

            _loggingWrapper = log;

        }

        public IEnumerable<string> Generate(
            IReadOnlyCollection<string> featureFiles,
            string projectFolder,
            string outputPath)
        {
            var codeBehindWriter = new CodeBehindWriter(null);

            if (featureFiles == null)
            {
                yield break;
            }

            foreach (var featureFile in featureFiles)
            {
                string featureFileItemSpec = featureFile;
                var generatorResult = _featureCodeBehindGenerator.GenerateCodeBehindFile(featureFileItemSpec);

                if (!generatorResult.Success)
                {
                    foreach (var error in generatorResult.Errors)
                    {
                        _loggingWrapper.LogError(error.Message);
                    }
                    continue;
                }

                string targetFilePath = _filePathGenerator.GenerateFilePath(
                    projectFolder,
                    outputPath,
                    featureFile,
                    generatorResult.Filename);

                string resultedFile = codeBehindWriter.WriteCodeBehindFile(targetFilePath, featureFile, generatorResult);

                yield return FileSystemHelper.GetRelativePath(resultedFile, projectFolder);
            }

        }

    }
}
