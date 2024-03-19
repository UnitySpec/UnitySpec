# Step Definitions

The step definitions provide the connection between your feature files and application interfaces. You have to add the `[Binding]` attribute to the class where your step definitions are:

```csharp
[Binding]
public class StepDefinitions
{
	...
}
```

> **Note:** Bindings are global for the entire SpecFlow project by default. Also see [the documentation on scoped bindings](ScopedBindings.md).

> See also the documentation on [ScenarioContext](ScenarioContext.md) and [FeatureContext](FeatureContext.md).

For better reusability, the step definitions can include parameters. This means that it is not necessary to define a new step definition for each step that just differs slightly. For example, the steps `When I perform a simple search on 'Domain'` and `When I perform a simple search on 'Communication'` can be automated with a single step definition, with 'Domain' and 'Communication' as parameters.  

The following example shows a simple step definition that matches to the step `When I perform a simple search on 'Domain'`:

``` csharp
[When(@"^I perform a simple search on '(.*)'$")]
public void WhenIPerformASimpleSearchOn(string searchTerm)
{
    var controller = new CatalogController();
    actionResult = controller.Search(searchTerm);
}
```

Here the method is annotated with the `[When]` attribute, and includes the regular expression used to match the step's text. This [regular expression](https://docs.microsoft.com/en-us/dotnet/standard/base-types/regular-expressions) uses (`(.*)`) to define parameters for the method.


## Supported Step Definition Attributes

* `[Given(expression)]`
* `[When(expression)]` 
* `[Then(expression)]` 
* `[StepDefinition(expression)]`, matches for given, when or then attributes

The `expression` should be a Regular Expression. 

You can annotate a single method with multiple attributes in order to support different phrasings in the feature file for the same automation logic.

```c#
[When("I perform a simple search on {string}")]
[When("I search for {string}")]
public void WhenIPerformASimpleSearchOn(string searchTerm)
{
  ...
}
```

## Other Attributes

The `[Obsolete]` attribute from the system namespace is also supported, check [here](https://docs.specflow.org/projects/specflow/en/latest/Installation/Configuration.html#runtime) for more details.

```c#
[Given("Stuff is done")]
[Obsolete]
public void GivenStuffIsDone()
{
    var x = 2+3;
}
```


## Step Definition Methods Rules

* Must be in a public class, marked with the `[Binding]` attribute.
* Must be a public method.
* Can be either a static or an instance method. If it is an instance method, the containing class will be instantiated once for every scenario.
* Cannot have `out` or `ref` parameters.
* Should return `void` or `IEnumerator`.

## Step Matching Styles & Rules


### Regular expressions in attributes

This is the classic and most used way of specifying the step definitions. The step definition method has to be annotated with one or more step definition attributes with regular expressions.

```c#
[Given(@"I have entered (.*) into the calculator")]
public void GivenIHaveEnteredNumberIntoTheCalculator(int number)
{
  ...
}
```

Regular expression matching rules:

* Regular expressions are always matched to the entire step, even if you do not use the `^` and `$` markers.
* The capturing groups (`(…)`) in the regular expression define the arguments for the method in order (the result of the first group becomes the first argument etc.).
* You can use non-capturing groups `(?:regex)` in order to use groups without a method argument.


## Parameter Matching Rules

* Step definitions can specify parameters. These will match to the parameters of the step definition method.
* The method parameter type can be `string` or other .NET type. In the later case a [configurable conversion](Step-Argument-Conversions.md) is applied.
* With regular expressions
  * The match groups (`(…)`) of the regular expression define the arguments for the method based on the order (the match result of the first group becomes the first argument, etc.).
  * You can use non-capturing groups `(?:regex)` in order to use groups without a method argument.

## Table or Multi-line Text Arguments

If the step definition method should match for steps having [table or multi-line text arguments](../Gherkin/Gherkin-Reference.md), additional `Table` and/or `string` parameters have to be defined in the method signature to be able to receive these arguments. If both table and multi-line text argument are used for the step, the multi-line text argument is provided first.

``` gherkin
Given the following books
  |Author        |Title                          |
  |Martin Fowler |Analysis Patterns              |
  |Gojko Adzic   |Bridging the Communication Gap |
```

``` csharp
[Given("the following books")]
public void GivenTheFollowingBooks(Table table)
{
  ...
}
```


>**Note** Big parts of this page where taken over from <https://github.com/SpecFlowOSS/SpecFlow/blob/master/docs/Bindings//Step-Definitions.md>.