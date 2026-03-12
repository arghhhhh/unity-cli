using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UnityCliBridge.TestScenes
{
    internal static class UnityCliPerfSceneGenerator
    {
        private const string MenuRoot = "Tools/Unity CLI/Performance/";
        private const string GeneratedRoot = "Assets/Scenes/Generated/E2E/Performance";
        private const string AssetRoot = GeneratedRoot + "/Assets";
        private const string ScenePath = GeneratedRoot + "/UnityCli_PerfBenchmark.unity";
        private const string TextureSourcePath = "Assets/Materials/Dice/DiceTexture.png";
        private const string TextureCopyPath = AssetRoot + "/UnityCli_PerfTexture.png";
        private const string BaseMaterialPath = AssetRoot + "/UnityCli_PerfBase.mat";
        private const string OverlayMaterialPath = AssetRoot + "/UnityCli_PerfOverlay.mat";

        [MenuItem(MenuRoot + "Generate Media Perf Scene")]
        private static void GenerateMediaPerfScene()
        {
            EnsureFolderExists("Assets/Scenes");
            EnsureFolderExists("Assets/Scenes/Generated");
            EnsureFolderExists("Assets/Scenes/Generated/E2E");
            EnsureFolderExists(GeneratedRoot);
            EnsureFolderExists(AssetRoot);

            string texturePath = EnsureTextureAsset();
            var baseMaterial = EnsureMaterial(BaseMaterialPath, texturePath, new Color(1f, 1f, 1f, 1f));
            var overlayMaterial = EnsureMaterial(OverlayMaterialPath, texturePath, new Color(1f, 1f, 1f, 0.75f));

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = EnsureMainCamera();
            EnsureDirectionalLight();

            var controllerObject = new GameObject("PerfScenarioController", typeof(UnityCliPerfScenarioController));
            var controller = controllerObject.GetComponent<UnityCliPerfScenarioController>();
            controller.mainCamera = camera;

            var staticRoot = new GameObject("StaticImageRoot");
            staticRoot.transform.SetParent(controllerObject.transform, false);
            CreateQuadGrid(staticRoot.transform, baseMaterial, 6, 4, 3.25f, 1.85f, new Vector3(-8.25f, 5f, 14f));

            var overlayRoot = new GameObject("OverlayRoot");
            overlayRoot.transform.SetParent(controllerObject.transform, false);
            CreateQuadGrid(overlayRoot.transform, overlayMaterial, 4, 2, 2.25f, 1.25f, new Vector3(-3.5f, -2.5f, 8f));
            overlayRoot.transform.rotation = Quaternion.Euler(0f, 0f, 8f);

            var motionRoot = new GameObject("MotionRoot");
            motionRoot.transform.SetParent(controllerObject.transform, false);
            CreateMotionRing(motionRoot.transform, baseMaterial);

            controller.staticImageRoot = staticRoot;
            controller.overlayRoot = overlayRoot;
            controller.motionRoot = motionRoot;
            controller.scenarioName = UnityCliPerfScenarioController.StaticImageScenario;
            controller.videoUrl = string.Empty;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static string EnsureTextureAsset()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(TextureCopyPath) == null)
            {
                AssetDatabase.CopyAsset(TextureSourcePath, TextureCopyPath);
                AssetDatabase.ImportAsset(TextureCopyPath);
            }

            var importer = AssetImporter.GetAtPath(TextureCopyPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Default;
                importer.mipmapEnabled = false;
                importer.maxTextureSize = 512;
                importer.textureCompression = TextureImporterCompression.Compressed;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }

            return TextureCopyPath;
        }

        private static Material EnsureMaterial(string materialPath, string texturePath, Color tint)
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                var shader = Shader.Find("Unlit/Texture") ?? Shader.Find("Standard");
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, materialPath);
            }

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                if (material.HasProperty("_MainTex"))
                {
                    material.SetTexture("_MainTex", texture);
                }

                if (material.HasProperty("_BaseMap"))
                {
                    material.SetTexture("_BaseMap", texture);
                }
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", tint);
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", tint);
            }

            EditorUtility.SetDirty(material);
            return material;
        }

        private static void CreateQuadGrid(Transform parent, Material material, int columns, int rows, float width, float height, Vector3 origin)
        {
            const float zStep = -0.05f;

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    int index = (row * columns) + column;
                    var quad = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    quad.name = $"Quad_{row}_{column}";
                    quad.transform.SetParent(parent, false);
                    quad.transform.position = origin + new Vector3(column * width, -row * height, zStep * index);
                    quad.transform.localScale = new Vector3(3f, 1.7f, 1f);

                    var renderer = quad.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.sharedMaterial = material;
                    }
                }
            }
        }

        private static void CreateMotionRing(Transform parent, Material material)
        {
            const int count = 12;
            const float radius = 6f;

            for (int i = 0; i < count; i++)
            {
                float angle = (360f / count) * i;
                var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.name = $"MotionCube_{i:00}";
                cube.transform.SetParent(parent, false);
                cube.transform.position = new Vector3(
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius,
                    Mathf.Sin(angle * Mathf.Deg2Rad) * 2f,
                    10f + Mathf.Sin(angle * Mathf.Deg2Rad) * 1.5f
                );
                cube.transform.rotation = Quaternion.Euler(angle, angle * 0.5f, 0f);
                cube.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);

                var renderer = cube.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    renderer.sharedMaterial = material;
                }
            }
        }

        private static Camera EnsureMainCamera()
        {
            var cameraObject = GameObject.FindWithTag("MainCamera");
            Camera camera;

            if (cameraObject == null)
            {
                cameraObject = new GameObject("Main Camera");
                cameraObject.tag = "MainCamera";
                camera = cameraObject.AddComponent<Camera>();
            }
            else
            {
                camera = cameraObject.GetComponent<Camera>();
                if (camera == null)
                {
                    camera = cameraObject.AddComponent<Camera>();
                }
            }

            camera.transform.position = new Vector3(0f, 0f, -12f);
            camera.transform.rotation = Quaternion.identity;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.05f, 0.06f, 0.1f, 1f);
            camera.fieldOfView = 48f;
            camera.nearClipPlane = 0.1f;
            camera.farClipPlane = 100f;
            return camera;
        }

        private static void EnsureDirectionalLight()
        {
            var lightObject = Object.FindFirstObjectByType<Light>();
            Light light;

            if (lightObject == null)
            {
                var go = new GameObject("Directional Light");
                light = go.AddComponent<Light>();
                light.type = LightType.Directional;
            }
            else
            {
                light = lightObject;
            }

            light.transform.rotation = Quaternion.Euler(45f, -35f, 0f);
            light.intensity = 1.15f;
        }

        private static void EnsureFolderExists(string assetPath)
        {
            if (AssetDatabase.IsValidFolder(assetPath))
            {
                return;
            }

            var parent = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            var folderName = Path.GetFileName(assetPath);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
