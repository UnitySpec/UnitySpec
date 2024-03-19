using Assets.Scripts;
using NUnit.Framework;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnitySpec;

namespace Specs.Tests
{
    [Binding]
    public class MoveStepDefinitions
    {
        private readonly ScenarioContext _scenarioContext;
        private string _sceneFolder;

        public MoveStepDefinitions(ScenarioContext scenarioContext)
        {
            _scenarioContext = scenarioContext;
        }

        [Given(@"I load the level ""(.*)""")]
        public IEnumerator GivenILoadTheLevel(string level)
        {
            var scene = AssetDatabase.GUIDToAssetPath(AssetDatabase.FindAssets(level).First());
            EditorSceneManager.LoadSceneInPlayMode(scene, new LoadSceneParameters(LoadSceneMode.Single));
            yield return null;
        }

        [Given(@"I have a position")]
        public void GivenIHaveAPosition()
        {
            var gameObject = GameObject.Find("Player");
            Assert.That(gameObject, Is.Not.Null, $"Player not found in {SceneManager.GetActiveScene().path}.");

            _scenarioContext.Add("position", gameObject.transform.position);
        }

        public IEnumerator WhenIWaitSecond(int seconds)
        {
            yield return new WaitForSeconds(seconds);
        }

        [When(@"I press (.) for (.) seconds?")]
        public IEnumerator WhenIPressXForYSeconds(string key, int seconds)
        {
            InputAbstraction.isFake = true;
            var faker = InputAbstraction.GetFake();
            faker.pressKey(key);
            yield return new WaitForSeconds(seconds);
            faker.releaseKey(key);
        }

        [Then(@"I have moved (.*)")]
        public IEnumerator ThenIHavedMoved(string direction)
        {
            var gameObject = GameObject.Find("Player");
            Assert.That(gameObject, Is.Not.Null, $"Player not found in {SceneManager.GetActiveScene().path}.");
            Vector3 currentPosition = gameObject.transform.position;
            Vector3 position = _scenarioContext.Get<Vector3>("position");
            switch (direction)
            {
                case "left":
                    Assert.That(currentPosition.x, Is.LessThan(position.x), $"{currentPosition.x} should be less than {position.x}");
                    break;
                case "right":
                    Assert.That(currentPosition.x, Is.GreaterThan(position.x), $"{currentPosition.x} should be greater than {position.x}");
                    break;
                case "forward":
                    Assert.That(currentPosition.z, Is.GreaterThan(position.z), $"{currentPosition.z} should be greater than {position.z}");
                    break;
                case "backward":
                    Assert.That(currentPosition.z, Is.LessThan(position.z), $"{currentPosition.z} should be less than {position.z}");
                    break;
                default:
                    Assert.IsTrue(false, $"Unknown direction in ThenIHavedMoved");
                    break;
            }
            yield return null;
        }


    }
}