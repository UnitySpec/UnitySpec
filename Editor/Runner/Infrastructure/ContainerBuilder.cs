using BoDi;
using System;
using System.Reflection;
using UnityFlow.Configuration;
using UnityFlow.Tracing;
using UnityFlow.UnitTestProvider;

namespace UnityFlow.Infrastructure
{
    public interface IContainerBuilder
    {
        IObjectContainer CreateGlobalContainer(Assembly testAssembly, IRuntimeConfigurationProvider configurationProvider = null);
        IObjectContainer CreateTestThreadContainer(IObjectContainer globalContainer);
        IObjectContainer CreateScenarioContainer(IObjectContainer testThreadContainer, ScenarioInfo scenarioInfo);
        IObjectContainer CreateFeatureContainer(IObjectContainer testThreadContainer, FeatureInfo featureInfo);
    }

    public class ContainerBuilder : IContainerBuilder
    {
        public static IDefaultDependencyProvider DefaultDependencyProvider = new DefaultDependencyProvider();

        private readonly IDefaultDependencyProvider _defaultDependencyProvider;

        public ContainerBuilder(IDefaultDependencyProvider defaultDependencyProvider = null)
        {
            _defaultDependencyProvider = defaultDependencyProvider ?? DefaultDependencyProvider;
        }

        public virtual IObjectContainer CreateGlobalContainer(Assembly testAssembly, IRuntimeConfigurationProvider configurationProvider = null)
        {
            var container = new ObjectContainer();
            container.RegisterInstanceAs<IContainerBuilder>(this);

            RegisterDefaults(container);

            var testAssemblyProvider = container.Resolve<ITestAssemblyProvider>();
            testAssemblyProvider.RegisterTestAssembly(testAssembly);

            if (configurationProvider != null)
                container.RegisterInstanceAs(configurationProvider);

            configurationProvider ??= container.Resolve<IRuntimeConfigurationProvider>();

            SpecFlowConfiguration specFlowConfiguration = ConfigurationLoader.GetDefault();
            specFlowConfiguration = configurationProvider.LoadConfiguration(specFlowConfiguration);
            //if (specFlowConfiguration.CustomDependencies != null)
            //{
            //    container.RegisterFromConfiguration(specFlowConfiguration.CustomDependencies);
            //}

            var unitTestProviderConfiguration = container.Resolve<UnitTestProviderConfiguration>();

            container.RegisterInstanceAs(specFlowConfiguration);

            if (unitTestProviderConfiguration != null)
                container.RegisterInstanceAs(container.Resolve<IUnitTestRuntimeProvider>(unitTestProviderConfiguration.UnitTestProvider ?? ConfigDefaults.UnitTestProviderName));
            
            container.Resolve<IConfigurationLoader>().TraceConfigSource(container.Resolve<ITraceListener>(), specFlowConfiguration);

            return container;
        }

        public virtual IObjectContainer CreateTestThreadContainer(IObjectContainer globalContainer)
        {
            var testThreadContainer = new ObjectContainer(globalContainer);

            _defaultDependencyProvider.RegisterTestThreadContainerDefaults(testThreadContainer);

            testThreadContainer.Resolve<ITestObjectResolver>();
            return testThreadContainer;
        }

        public virtual IObjectContainer CreateScenarioContainer(IObjectContainer testThreadContainer, ScenarioInfo scenarioInfo)
        {
            if (testThreadContainer == null)
                throw new ArgumentNullException(nameof(testThreadContainer));

            var scenarioContainer = new ObjectContainer(testThreadContainer);
            scenarioContainer.RegisterInstanceAs(scenarioInfo);
            _defaultDependencyProvider.RegisterScenarioContainerDefaults(scenarioContainer);

            scenarioContainer.ObjectCreated += obj =>
            {
                if (obj is IContainerDependentObject containerDependentObject)
                {
                    containerDependentObject.SetObjectContainer(scenarioContainer);
                }
            };


            return scenarioContainer;
        }

        public IObjectContainer CreateFeatureContainer(IObjectContainer testThreadContainer, FeatureInfo featureInfo)
        {
            if (testThreadContainer == null)
                throw new ArgumentNullException(nameof(testThreadContainer));

            var featureContainer = new ObjectContainer(testThreadContainer);
            featureContainer.RegisterInstanceAs(featureInfo);

            featureContainer.ObjectCreated += obj =>
            {
                if (obj is IContainerDependentObject containerDependentObject)
                {
                    containerDependentObject.SetObjectContainer(featureContainer);
                }
            };

            return featureContainer;
        }


        protected virtual void RegisterDefaults(ObjectContainer container)
        {
            _defaultDependencyProvider.RegisterGlobalContainerDefaults(container);

            container.RegisterTypeAs<UTFRuntimeProvider, IUnitTestRuntimeProvider>("utf");
        }
    }
}