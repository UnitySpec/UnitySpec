using UnityEngine.Assertions;
using UnitySpec;

[Binding]
public class AdditionStepDefinition
{
    private readonly ScenarioContext _scenarioContext;

    public AdditionStepDefinition(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [Given(@"the first number is (.*)")]
    public void GivenTheFirstNumberIs(int number)
    {
        _scenarioContext.Add("firstNumber", number);
    }
    [Given(@"the second number is (.*)")]
    public void GivenTheSecondNumberIs(int number)
    {
        _scenarioContext.Add("secondNumber", number);
    }

    [When(@"the two numbers are added")]
    public void WhenTheTwoNumbersAreAdded()
    {
        int firstNumber = _scenarioContext.Get<int>("firstNumber");
        int secondNumber = _scenarioContext.Get<int>("secondNumber");
        int result = firstNumber + secondNumber;

        _scenarioContext.Add("result", result);
    }
    [Then(@"the result should be (.*)")]
    public void ThenTheResultShouldBe(int number)
    {
        int result = _scenarioContext.Get<int>("result");
        Assert.AreEqual(number, result);
    }

}
