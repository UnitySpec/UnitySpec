using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using BoDi;
using UnityFlow.BindingSkeletons;
using UnityFlow.Configuration.JsonConfig;
using UnityFlow.Tracing;
using UnityFlow.Compatibility;

namespace UnityFlow.Configuration
{
    public class ConfigurationLoader : IConfigurationLoader
    {

        private readonly JsonConfigurationLoader _jsonConfigurationLoader;
        private readonly ISpecFlowJsonLocator _specFlowJsonLocator;

        public ConfigurationLoader(ISpecFlowJsonLocator specFlowJsonLocator)
        {
            _specFlowJsonLocator = specFlowJsonLocator;
            _jsonConfigurationLoader = new JsonConfigurationLoader();
        }

        private static CultureInfo DefaultFeatureLanguage => CultureInfo.GetCultureInfo(ConfigDefaults.FeatureLanguage);

        private static CultureInfo DefaultToolLanguage => CultureInfoHelper.GetCultureInfo(ConfigDefaults.FeatureLanguage);

        private static CultureInfo DefaultBindingCulture => null;
        private static string DefaultUnitTestProvider => ConfigDefaults.UnitTestProviderName;
        private static bool DefaultDetectAmbiguousMatches => ConfigDefaults.DetectAmbiguousMatches;
        private static bool DefaultStopAtFirstError => ConfigDefaults.StopAtFirstError;

        private static MissingOrPendingStepsOutcome DefaultMissingOrPendingStepsOutcome => ConfigDefaults.MissingOrPendingStepsOutcome;

        private static bool DefaultTraceSuccessfulSteps => ConfigDefaults.TraceSuccessfulSteps;
        private static bool DefaultTraceTimings => ConfigDefaults.TraceTimings;
        private static TimeSpan DefaultMinTracedDuration => TimeSpan.Parse(ConfigDefaults.MinTracedDuration);

        private static StepDefinitionSkeletonStyle DefaultStepDefinitionSkeletonStyle => ConfigDefaults.StepDefinitionSkeletonStyle;

        private static List<string> DefaultAdditionalStepAssemblies => new List<string>();
        private static bool DefaultAllowDebugGeneratedFiles => ConfigDefaults.AllowDebugGeneratedFiles;
        private static bool DefaultAllowRowTests => ConfigDefaults.AllowRowTests;
        public static string DefaultGeneratorPath => ConfigDefaults.GeneratorPath;

        public static string[] DefaultAddNonParallelizableMarkerForTags => ConfigDefaults.AddNonParallelizableMarkerForTags;

        public static ObsoleteBehavior DefaultObsoleteBehavior => ConfigDefaults.ObsoleteBehavior;

        public bool HasAppConfig = false;

        public bool HasJsonConfig
        {
            get
            {
                var specflowJsonFile = _specFlowJsonLocator.GetSpecFlowJsonFilePath();
                return File.Exists(specflowJsonFile);
            }
        }

        public SpecFlowConfiguration Load(SpecFlowConfiguration specFlowConfiguration, ISpecFlowConfigurationHolder specFlowConfigurationHolder)
        {
            if (!specFlowConfigurationHolder.HasConfiguration)
                return GetDefault();

            return specFlowConfigurationHolder.ConfigSource switch
            {
                ConfigSource.Default => GetDefault(),
                ConfigSource.Json => LoadJson(specFlowConfiguration, specFlowConfigurationHolder.Content),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        public SpecFlowConfiguration Load(SpecFlowConfiguration specFlowConfiguration)
        {
            if (HasJsonConfig)
                return LoadJson(specFlowConfiguration);

            return GetDefault();
        }


        public void TraceConfigSource(ITraceListener traceListener, SpecFlowConfiguration specFlowConfiguration)
        {
            switch (specFlowConfiguration.ConfigSource)
            {
                case ConfigSource.Default:
                    traceListener.WriteToolOutput("Using default config");
                    break;
                case ConfigSource.Json:
                    traceListener.WriteToolOutput("Using specflow.json");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public static SpecFlowConfiguration GetDefault()
        {
            return new SpecFlowConfiguration(ConfigSource.Default,
                new ContainerRegistrationCollection(),
                new ContainerRegistrationCollection(),
                DefaultFeatureLanguage,
                DefaultBindingCulture,
                DefaultStopAtFirstError,
                DefaultMissingOrPendingStepsOutcome,
                DefaultTraceSuccessfulSteps,
                DefaultTraceTimings,
                DefaultMinTracedDuration,
                DefaultStepDefinitionSkeletonStyle,
                DefaultAdditionalStepAssemblies,
                DefaultAllowDebugGeneratedFiles,
                DefaultAllowRowTests,
                DefaultAddNonParallelizableMarkerForTags,
                DefaultObsoleteBehavior
                );
        }


        private SpecFlowConfiguration LoadJson(SpecFlowConfiguration specFlowConfiguration)
        {
            var jsonContent = File.ReadAllText(_specFlowJsonLocator.GetSpecFlowJsonFilePath());

            return LoadJson(specFlowConfiguration, jsonContent);
        }

        private SpecFlowConfiguration LoadJson(SpecFlowConfiguration specFlowConfiguration, string jsonContent)
        {
            return _jsonConfigurationLoader.LoadJson(specFlowConfiguration, jsonContent);
        }


    }
}
