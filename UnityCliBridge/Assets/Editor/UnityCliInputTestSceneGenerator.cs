using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace UnityCliBridge.TestScenes
{
    public static class UnityCliInputTestSceneGenerator
    {
        private const string MenuRoot = "Tools/Unity CLI/Input Tests/";
        private const string ScenesFolder = "Assets/Scenes/Generated/E2E";
        private const string ScenePath = ScenesFolder + "/UnityCli_InputSimulation_TestScene.unity";

        [MenuItem(MenuRoot + "Generate Input Simulation Test Scene")]
        private static void GenerateInputSimulationTestScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("InputE2E_Root");
            root.AddComponent<UnityCliInputSimulationTestBootstrap>();

            EditorSceneManager.MarkSceneDirty(scene);
            EnsureFolderExists(ScenesFolder);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ConfigureBridgePortFromEnv()
        {
            var rawPort = System.Environment.GetEnvironmentVariable("UNITY_CLI_PORT_OVERRIDE");
            if (string.IsNullOrWhiteSpace(rawPort))
            {
                rawPort = System.Environment.GetEnvironmentVariable("UNITY_CLI_PORT");
            }

            if (!int.TryParse(rawPort, out var port) || port < 1 || port > 65535)
            {
                Debug.LogError($"UNITY_CLI_PORT_OVERRIDE/UNITY_CLI_PORT is invalid: '{rawPort}'");
                return;
            }

            var settingsType = System.Type.GetType("UnityCliBridge.Settings.UnityCliBridgeProjectSettings, UnityCliBridge.Editor");
            if (settingsType == null)
            {
                Debug.LogError("UnityCliBridgeProjectSettings type not found.");
                return;
            }

            var instanceProperty =
                settingsType.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy) ??
                settingsType.BaseType?.GetProperty("instance", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var settings = instanceProperty?.GetValue(null);
            if (settings == null)
            {
                Debug.LogError("UnityCliBridgeProjectSettings.instance is unavailable.");
                return;
            }

            settingsType.GetMethod("SetPort", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(settings, new object[] { port });
            settingsType.GetMethod("SaveProjectSettings", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(settings, new object[] { true });
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Unity CLI Bridge port saved to {port}");
        }

        private static void EnsureFolderExists(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace("\\", "/");
            var name = Path.GetFileName(assetPath);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name))
            {
                return;
            }

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
