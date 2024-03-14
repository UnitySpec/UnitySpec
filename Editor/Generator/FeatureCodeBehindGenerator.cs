﻿using System;
using System.IO;
using UnitySpec.General.GeneratorInterfaces;

namespace UnitySpec.Generator
{
    public class FeatureCodeBehindGenerator : IDisposable
    {
        private readonly ITestGenerator _testGenerator;

        public FeatureCodeBehindGenerator(ITestGenerator testGenerator)
        {
            _testGenerator = testGenerator;
        }

        public TestFileGeneratorResult GenerateCodeBehindFile(string featureFile)
        {
            var featureFileInput = new FeatureFileInput(featureFile);

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
