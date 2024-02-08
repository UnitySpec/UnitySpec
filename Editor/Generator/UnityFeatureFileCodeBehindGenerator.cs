using SpecFlow.Tools.MsBuild.Generation;
using System.Collections.Generic;
using TechTalk.SpecFlow.Utils;
using UnityEngine;

namespace UnityFlow.Generator
{
    public class UnityFeatureFileCodeBehindGenerator : IFeatureFileCodeBehindGenerator
    {
        private readonly FilePathGenerator _filePathGenerator;
        private readonly UnityFeatureCodeBehindGenerator _featureCodeBehindGenerator;

        public UnityFeatureFileCodeBehindGenerator(UnityFeatureCodeBehindGenerator featureCodeBehindGenerator)
        {
            _featureCodeBehindGenerator = featureCodeBehindGenerator;
            _filePathGenerator = new FilePathGenerator();
        }

        public IEnumerable<string> GenerateFilesForProject(
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
                        UnityEngine.Debug.LogError(error);
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
