using BoDi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnitySpec.Bindings;
using UnitySpec.ErrorHandling;
using UnitySpec.Infrastructure;


namespace UnitySpec
{
    public interface IScenarioContext : ISpecFlowContext
    {
        ScenarioInfo ScenarioInfo { get; }

        ScenarioBlock CurrentScenarioBlock { get; }

        IObjectContainer ScenarioContainer { get; }

        ScenarioExecutionStatus ScenarioExecutionStatus { get; }
    }

    public class ScenarioContext : SpecFlowContext, IScenarioContext
    {
        #region Singleton
        private static bool isCurrentDisabled = false;
        private static ScenarioContext current;

        [Obsolete("Please get the ScenarioContext via Context Injection - https://go.specflow.org/Migrate-ScenarioContext-Current")]
        public static ScenarioContext Current
        {
            get
            {
                if (isCurrentDisabled)
                    throw new SpecFlowException("The ScenarioContext.Current static accessor cannot be used in multi-threaded execution. Try injecting the scenario context to the binding class. See https://go.specflow.org/doc-multithreaded for details.");
                if (current == null)
                {
                    Debug.WriteLine("Accessing NULL ScenarioContext");
                }
                return current;
            }
            internal set
            {
                if (!isCurrentDisabled)
                    current = value;
            }
        }

        internal static void DisableSingletonInstance()
        {
            isCurrentDisabled = true;
            Thread.MemoryBarrier();
            current = null;
        }
        #endregion

        public ScenarioInfo ScenarioInfo { get; }
        public ScenarioBlock CurrentScenarioBlock { get; internal set; }
        public IObjectContainer ScenarioContainer { get; }

        public ScenarioExecutionStatus ScenarioExecutionStatus { get; internal set; }
        internal List<string> PendingSteps { get; }
        internal List<StepInstance> MissingSteps { get; }
        internal Stopwatch Stopwatch { get; }

        private readonly ITestObjectResolver testObjectResolver;

        internal ScenarioContext(IObjectContainer scenarioContainer, ScenarioInfo scenarioInfo, ITestObjectResolver testObjectResolver)
        {
            this.ScenarioContainer = scenarioContainer;
            this.testObjectResolver = testObjectResolver;

            Stopwatch = new Stopwatch();
            Stopwatch.Start();

            CurrentScenarioBlock = ScenarioBlock.None;
            ScenarioInfo = scenarioInfo;
            ScenarioExecutionStatus = ScenarioExecutionStatus.OK;
            PendingSteps = new List<string>();
            MissingSteps = new List<StepInstance>();
        }

        public ScenarioStepContext StepContext => ScenarioContainer.Resolve<IContextManager>().StepContext;

        public void Pending()
        {
            throw new PendingStepException();
        }

        public static void StepIsPending()
        {
            throw new PendingStepException();
        }

        /// <summary>
        /// Called by SpecFlow infrastructure when an instance of a binding class is needed.
        /// </summary>
        /// <param name="bindingType">The type of the binding class.</param>
        /// <returns>The binding class instance</returns>
        /// <remarks>
        /// The binding classes are the classes with the [Binding] attribute, that might
        /// contain step definitions, hooks or step argument transformations. The method
        /// is called when any binding method needs to be called.
        /// </remarks>
        public object GetBindingInstance(Type bindingType)
        {
            return testObjectResolver.ResolveBindingInstance(bindingType, ScenarioContainer);
        }
    }
}