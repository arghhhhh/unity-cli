using System.Linq;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using UnityCliBridge.Handlers;

namespace UnityCliBridge.Tests
{
    [TestFixture]
    public class AssetManagementHandlerTests
    {
        private const string TestFolder = "Assets/UnityCliBridgeTests/AnimatorControllers";

        [SetUp]
        public void Setup()
        {
            EnsureFolder("Assets/UnityCliBridgeTests");
            EnsureFolder(TestFolder);
        }

        [TearDown]
        public void TearDown()
        {
            if (AssetDatabase.IsValidFolder("Assets/UnityCliBridgeTests"))
            {
                AssetDatabase.DeleteAsset("Assets/UnityCliBridgeTests");
                AssetDatabase.Refresh();
            }
        }

        [Test]
        public void CreateAnimatorController_WithStatesParametersAndTransitions_ReturnsSuccess()
        {
            string idlePath = CreateClipAsset("Idle");
            string runPath = CreateClipAsset("Run");
            string controllerPath = TestFolder + "/Hero.controller";

            var parameters = new JObject
            {
                ["controllerPath"] = controllerPath,
                ["parameters"] = new JArray
                {
                    new JObject
                    {
                        ["name"] = "isMoving",
                        ["type"] = "Bool",
                        ["defaultBool"] = false
                    }
                },
                ["states"] = new JArray
                {
                    new JObject
                    {
                        ["name"] = "Idle",
                        ["motionPath"] = idlePath
                    },
                    new JObject
                    {
                        ["name"] = "Run",
                        ["motionPath"] = runPath
                    }
                },
                ["defaultState"] = "Idle",
                ["transitions"] = new JArray
                {
                    new JObject
                    {
                        ["from"] = "Idle",
                        ["to"] = "Run",
                        ["conditions"] = new JArray
                        {
                            new JObject
                            {
                                ["parameter"] = "isMoving",
                                ["mode"] = "If"
                            }
                        }
                    }
                }
            };

            JObject result = ToJObject(AssetManagementHandler.CreateAnimatorController(parameters));

            Assert.IsNull(result.Value<string>("error"));
            Assert.IsTrue(result.Value<bool>("success"));
            Assert.AreEqual(controllerPath, result.Value<string>("controllerPath"));
            Assert.AreEqual("Idle", result.Value<string>("defaultState"));
            Assert.AreEqual(1, result.Value<int>("parameterCount"));
            Assert.AreEqual(2, result.Value<int>("stateCount"));
            Assert.AreEqual(1, result.Value<int>("transitionCount"));

            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(controllerPath);
            Assert.IsNotNull(controller);
            Assert.AreEqual(1, controller.parameters.Length);
            Assert.AreEqual("isMoving", controller.parameters[0].name);
            Assert.AreEqual(AnimatorControllerParameterType.Bool, controller.parameters[0].type);
            Assert.IsFalse(controller.parameters[0].defaultBool);

            AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
            AnimatorState idleState = FindState(stateMachine, "Idle");
            AnimatorState runState = FindState(stateMachine, "Run");

            Assert.IsNotNull(idleState);
            Assert.IsNotNull(runState);
            Assert.AreEqual("Idle", stateMachine.defaultState.name);
            Assert.AreEqual(idlePath, AssetDatabase.GetAssetPath(idleState.motion));
            Assert.AreEqual(runPath, AssetDatabase.GetAssetPath(runState.motion));

            AnimatorStateTransition transition = idleState.transitions.Single();
            Assert.AreEqual(runState, transition.destinationState);
            Assert.AreEqual(1, transition.conditions.Length);
            Assert.AreEqual("isMoving", transition.conditions[0].parameter);
            Assert.AreEqual(AnimatorConditionMode.If, transition.conditions[0].mode);
        }

        [Test]
        public void CreateAnimatorController_WithExistingPathAndOverwriteFalse_ReturnsError()
        {
            string controllerPath = TestFolder + "/Existing.controller";
            AnimatorController.CreateAnimatorControllerAtPath(controllerPath);

            JObject result = ToJObject(AssetManagementHandler.CreateAnimatorController(new JObject
            {
                ["controllerPath"] = controllerPath
            }));

            Assert.AreEqual("AnimatorController request validation failed", result.Value<string>("error"));
            StringAssert.Contains(
                "AnimatorController already exists",
                result["validationErrors"]?[0]?.ToString()
            );
        }

        [Test]
        public void CreateAnimatorController_WithMissingMotionClip_ReturnsError()
        {
            JObject result = ToJObject(AssetManagementHandler.CreateAnimatorController(new JObject
            {
                ["controllerPath"] = TestFolder + "/MissingClip.controller",
                ["states"] = new JArray
                {
                    new JObject
                    {
                        ["name"] = "Idle",
                        ["motionPath"] = TestFolder + "/Missing.anim"
                    }
                }
            }));

            Assert.AreEqual("AnimatorController request validation failed", result.Value<string>("error"));
            StringAssert.Contains(
                "does not point to a Motion asset",
                result["validationErrors"]?[0]?.ToString()
            );
        }

        [Test]
        public void CreateAnimatorController_WithDuplicateStateNames_ReturnsError()
        {
            JObject result = ToJObject(AssetManagementHandler.CreateAnimatorController(new JObject
            {
                ["controllerPath"] = TestFolder + "/DuplicateState.controller",
                ["states"] = new JArray
                {
                    new JObject { ["name"] = "Idle" },
                    new JObject { ["name"] = "Idle" }
                }
            }));

            Assert.AreEqual("AnimatorController request validation failed", result.Value<string>("error"));
            StringAssert.Contains("Duplicate state name: Idle", result["validationErrors"]?.ToString());
        }

        [Test]
        public void CreateAnimatorController_WithUnknownTransitionParameter_ReturnsError()
        {
            JObject result = ToJObject(AssetManagementHandler.CreateAnimatorController(new JObject
            {
                ["controllerPath"] = TestFolder + "/UnknownParameter.controller",
                ["states"] = new JArray
                {
                    new JObject { ["name"] = "Idle" },
                    new JObject { ["name"] = "Run" }
                },
                ["transitions"] = new JArray
                {
                    new JObject
                    {
                        ["from"] = "Idle",
                        ["to"] = "Run",
                        ["conditions"] = new JArray
                        {
                            new JObject
                            {
                                ["parameter"] = "isMoving",
                                ["mode"] = "If"
                            }
                        }
                    }
                }
            }));

            Assert.AreEqual("AnimatorController request validation failed", result.Value<string>("error"));
            StringAssert.Contains("references unknown parameter: isMoving", result["validationErrors"]?.ToString());
        }

        private static JObject ToJObject(object result)
        {
            return result as JObject ?? JObject.FromObject(result);
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
            {
                return;
            }

            string parent = path.Substring(0, path.LastIndexOf('/'));
            string folderName = path.Substring(path.LastIndexOf('/') + 1);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        private static string CreateClipAsset(string name)
        {
            string path = $"{TestFolder}/{name}.anim";
            var clip = new AnimationClip();
            AssetDatabase.CreateAsset(clip, path);
            AssetDatabase.SaveAssets();
            return path;
        }

        private static AnimatorState FindState(AnimatorStateMachine stateMachine, string stateName)
        {
            return stateMachine.states
                .Select(child => child.state)
                .SingleOrDefault(state => state.name == stateName);
        }
    }
}
