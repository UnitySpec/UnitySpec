using SpecFlow.Tools.MsBuild.Generation;
using System;
using System.IO;
using TechTalk.SpecFlow.Generator.Interfaces;
using UnityEngine;

namespace UnityFlow.Generator
{
    public class UnityFeatureCodeBehindGenerator : IDisposable
    {
        private readonly ITestGenerator _testGenerator;

        public UnityFeatureCodeBehindGenerator(ITestGenerator testGenerator)
        {
            _testGenerator = testGenerator;
        }

        public TestFileGeneratorResult GenerateCodeBehindFile(string featureFile)
        {
            var featureFileInput = new FeatureFileInput(featureFile);
            featureFileInput.CustomNamespace = "Specs.Tests.Features";

            var generatedFeatureFileName = Path.GetFileName(_testGenerator.GetTestFullPath(featureFileInput));

            var testGeneratorResult = _testGenerator.GenerateTestFile(featureFileInput, new GenerationSettings());

            return new TestFileGeneratorResult(testGeneratorResult, generatedFeatureFileName);
        }

        public void Dispose()
        {
            _testGenerator?.Dispose();
        }

    }
}
