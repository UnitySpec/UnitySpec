﻿using System;
using System.Collections.Generic;
using UnitySpec.General.GeneratorInterfaces;

namespace UnitySpec.Generator
{
    public class TestGeneratorFactory : RemotableGeneratorClass, ITestGeneratorFactory
    {
        public static readonly Version GeneratorVersion = typeof(TestGeneratorFactory).Assembly.GetName().Version;
        public Version GetGeneratorVersion()
        {
            return GeneratorVersion;
        }

        public ITestGenerator CreateGenerator(ProjectSettings projectSettings, IEnumerable<GeneratorPluginInfo> generatorPluginInfos)
        {
            var container = new GeneratorContainerBuilder().CreateContainer(projectSettings.ConfigurationHolder, projectSettings, generatorPluginInfos);
            return container.Resolve<ITestGenerator>();
        }
    }
}
