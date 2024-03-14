using BoDi;
using UnitySpec.General.Configuration;
using UnitySpec.General.Configuration.Interfaces;
using UnitySpec.General.GeneratorInterfaces;
using UnitySpec.General.Parser;
using UnitySpec.General.Utils;
using UnitySpec.Generator.Configuration;
using UnitySpec.Generator.Generation;
using UnitySpec.Generator.Plugins;
using UnitySpec.Generator.UnitTestConverter;

namespace UnitySpec.Generator
{
    internal partial class DefaultDependencyProvider
    {
        partial void RegisterUnitTestGeneratorProviders(ObjectContainer container);

        public virtual void RegisterDefaults(ObjectContainer container)
        {
            container.RegisterTypeAs<FileSystem, IFileSystem>();

            container.RegisterTypeAs<GeneratorConfigurationProvider, IGeneratorConfigurationProvider>();
            container.RegisterTypeAs<InProcGeneratorInfoProvider, IGeneratorInfoProvider>();
            container.RegisterTypeAs<TestGenerator, ITestGenerator>();
            container.RegisterTypeAs<TestHeaderWriter, ITestHeaderWriter>();
            container.RegisterTypeAs<TestUpToDateChecker, ITestUpToDateChecker>();

            container.RegisterTypeAs<GeneratorPluginLoader, IGeneratorPluginLoader>();
            //container.RegisterTypeAs<DefaultListener, ITraceListener>();

            container.RegisterTypeAs<UnitTestFeatureGenerator, UnitTestFeatureGenerator>();
            container.RegisterTypeAs<FeatureGeneratorRegistry, IFeatureGeneratorRegistry>();
            container.RegisterTypeAs<UnitTestFeatureGeneratorProvider, IFeatureGeneratorProvider>("default");
            container.RegisterTypeAs<TagFilterMatcher, ITagFilterMatcher>();

            container.RegisterTypeAs<DecoratorRegistry, IDecoratorRegistry>();
            container.RegisterTypeAs<IgnoreDecorator, ITestClassTagDecorator>("ignore");
            container.RegisterTypeAs<IgnoreDecorator, ITestMethodTagDecorator>("ignore");
            container.RegisterTypeAs<NonParallelizableDecorator, ITestClassDecorator>("nonparallelizable");

            container.RegisterInstanceAs(GenerationTargetLanguage.CreateCodeDomHelper(GenerationTargetLanguage.CSharp), GenerationTargetLanguage.CSharp, dispose: true);
            container.RegisterInstanceAs(GenerationTargetLanguage.CreateCodeDomHelper(GenerationTargetLanguage.VB), GenerationTargetLanguage.VB, dispose: true);

            container.RegisterTypeAs<ConfigurationLoader, IConfigurationLoader>();

            container.RegisterTypeAs<SpecFlowGherkinParserFactory, IGherkinParserFactory>();

            container.RegisterTypeAs<SpecFlowJsonLocator, ISpecFlowJsonLocator>();

            RegisterUnitTestGeneratorProviders(container);
        }
    }
}
