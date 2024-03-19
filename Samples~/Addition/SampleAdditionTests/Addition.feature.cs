﻿// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by UnitySpec (https://github.com/mmulder135/UnitySpec).
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace Samples.SimpleAddition.SampleAdditionTests
{
    using UnitySpec;
    using System;
    using System.Linq;

    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("Addition")]
    public partial class AdditionFeature
    {
        private static string[] featureTags = ((string[])(null));
        [UnityEngine.TestTools.UnityTestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Add two numbers")]
        public System.Collections.IEnumerator AddTwoNumbers()
        {
            string[] tagsOfScenario = ((string[])(null));
            System.Collections.Specialized.OrderedDictionary argumentsOfScenario = new System.Collections.Specialized.OrderedDictionary();
            UnitySpec.ScenarioInfo scenarioInfo = new UnitySpec.ScenarioInfo("Add two numbers", null, tagsOfScenario, argumentsOfScenario, featureTags);
#line 4
            this.ScenarioInitialize(scenarioInfo);
            if ((TagHelper.ContainsIgnoreTag(tagsOfScenario) || TagHelper.ContainsIgnoreTag(featureTags)))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
#line 5
                yield return ((System.Collections.IEnumerator)(testRunner.Given("the first number is 50", (string)(null), (UnitySpec.Table)null, "Given ")));
#line 6
                yield return ((System.Collections.IEnumerator)(testRunner.And("the second number is 70", (string)(null), (UnitySpec.Table)null, "And ")));
#line 7
                yield return ((System.Collections.IEnumerator)(testRunner.When("the two numbers are added", (string)(null), (UnitySpec.Table)null, "When ")));
#line 8
                yield return ((System.Collections.IEnumerator)(testRunner.Then("the result should be 120", (string)(null), (UnitySpec.Table)null, "Then ")));
            }

            this.ScenarioCleanup();
        }

        private UnitySpec.ITestRunner testRunner;
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public void FeatureSetup()
        {
            testRunner = UnitySpec.TestRunnerManager.GetTestRunner(null, 0);
            UnitySpec.FeatureInfo featureInfo = new UnitySpec.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "C:/Users/Michael/Documents/Unity Learn/My project/Assets/Samples/UnitySpec/0.0.2/Simple addition/SampleAdditionTests", "Addition", null, ProgrammingLanguage.CSharp, featureTags);
            testRunner.OnFeatureStart(featureInfo);
        }

        public void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }

        [NUnit.Framework.SetUpAttribute()]
        public void TestInitialize()
        {
        }

        [NUnit.Framework.TearDownAttribute()]
        public void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }

        public void ScenarioInitialize(UnitySpec.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }

        public void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }

        public void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
    }
}
#pragma warning restore
#endregion