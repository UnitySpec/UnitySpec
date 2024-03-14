# UnitySpec
UnitySpec is a BDD solution for Unity3D. It is based on the Gherkin specification language. 
This project is based on the code from [SpecFlow](https://github.com/SpecFlowOSS/SpecFlow).
> UnitySpec has been tested in Unity 2022.3 for both .Net Standard 2.1 and .Net Framework

## Installation
 - Recommended installation through the Unity Package Manager
	 - Open Unity Package Manager
	 - Go to `+` -> `Add package from git URL..` and enter `https://github.com/mmulder135/UnitySpec.git`
 > UnitySpec requires the Unity Test Framework to run tests

## Setup
1. Create a test folder in Unity
> This can either be a EditMode or PlayMode testfolder,
> see the [documentation from the Unity Test Framework](https://docs.unity3d.com/Packages/com.unity.test-framework@1.1/manual/workflow-create-test-assembly.html) for more information on test folders.

2. Optional: Create folders for feature files and step definition files inside the test folder.
3. Open Settings and navigate to UnitySpec, set the folder containing the feature files under Specification folders.
> You can add multiple paths for multiple folders. You can also edit the search behaviour, default is searching the listed directories and all their subdirectories.


## Usage
1. **Write scenarios**
   After installation feature files can be added in Unity by right-clicking in the project window > Create > New Feature.
Scenarios should be written in Gherkin, [a Gherkin reference can be found here](Docs/GherkinReference.md).

2. **Create test files**
Once the scenario is written UnitySpec can be used to autogenerate test files to test this scenario.
Go to Window > UnitySpec to open the UnitySpec Window. Click on the button to generate test files.
The files are generated at the same location as the feature files.

3. **Write step definitions**
For each step used in the scenarios, a step definition has to be written. See [the documentation on step definitions](Docs/StepDefinitions.md) for more information.
   
5. **Run test files**
Open the test runner. For each feature file a test file should be visible, and for each scenario a test in that file.
Double click a test to run it, or press the play button to run all.
	> If you see no tests you might need to refresh. Do this by going to the project window and pressing `ctrl + R`, alternatively, right-click > Refresh, or clicking outside Unity and going back.
