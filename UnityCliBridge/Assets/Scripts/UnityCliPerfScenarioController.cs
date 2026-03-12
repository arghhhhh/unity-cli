using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Video;

namespace UnityCliBridge.TestScenes
{
    public sealed class UnityCliPerfScenarioController : MonoBehaviour
    {
        public const string StaticImageScenario = "StaticImage";
        public const string SourceMotionScenario = "SourceMotion";
        public const string VideoPlaybackScenario = "VideoPlayback";
        public const string MixedOverlayScenario = "MixedOverlay";

        [Header("Scenario")]
        public string scenarioName = StaticImageScenario;
        public string videoUrl = string.Empty;

        [Header("Scene Roots")]
        public GameObject staticImageRoot;
        public GameObject overlayRoot;
        public GameObject motionRoot;
        public Camera mainCamera;

        [Header("Motion")]
        public Vector3 motionAxis = new Vector3(0f, 1f, 0f);
        public float motionDegreesPerSecond = 20f;
        public float overlayDegreesPerSecond = 8f;

        [Header("Video")]
        public bool autoPlayVideo = true;
        public float videoPlaybackSpeed = 1f;

        private VideoPlayer videoPlayer;
        private Coroutine playRoutine;

        private void Awake()
        {
            EnsureBindings();
            ApplyScenario();
        }

        private void OnEnable()
        {
            EnsureBindings();
            ApplyScenario();
        }

        private void OnValidate()
        {
            EnsureBindings();
            ApplyScenario();
        }

        private void Update()
        {
            if (motionRoot != null && motionRoot.activeSelf)
            {
                motionRoot.transform.Rotate(motionAxis, motionDegreesPerSecond * Time.deltaTime, Space.World);
            }

            if (overlayRoot != null && overlayRoot.activeSelf)
            {
                overlayRoot.transform.Rotate(Vector3.forward, overlayDegreesPerSecond * Time.deltaTime, Space.Self);
            }
        }

        private void ApplyScenario()
        {
            EnsureBindings();

            bool showStatic = MatchesScenario(StaticImageScenario) || MatchesScenario(MixedOverlayScenario);
            bool showOverlay = MatchesScenario(MixedOverlayScenario);
            bool showMotion = MatchesScenario(SourceMotionScenario);
            bool enableVideo = MatchesScenario(VideoPlaybackScenario) || MatchesScenario(MixedOverlayScenario);

            SetActive(staticImageRoot, showStatic);
            SetActive(overlayRoot, showOverlay);
            SetActive(motionRoot, showMotion);

            ConfigureVideo(enableVideo);
        }

        private bool MatchesScenario(string expected)
        {
            return string.Equals(scenarioName?.Trim(), expected, StringComparison.OrdinalIgnoreCase);
        }

        private void ConfigureVideo(bool enableVideo)
        {
            EnsureCamera();

            if (!enableVideo || string.IsNullOrWhiteSpace(videoUrl) || mainCamera == null)
            {
                StopVideoPlayer();
                return;
            }

            if (videoPlayer == null)
            {
                videoPlayer = mainCamera.GetComponent<VideoPlayer>();
                if (videoPlayer == null)
                {
                    videoPlayer = mainCamera.gameObject.AddComponent<VideoPlayer>();
                }
            }

            videoPlayer.enabled = true;
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = NormalizeVideoUrl(videoUrl);
            videoPlayer.playOnAwake = autoPlayVideo;
            videoPlayer.isLooping = true;
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = mainCamera;
            videoPlayer.targetCameraAlpha = 1f;
            videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
            videoPlayer.skipOnDrop = true;
            videoPlayer.waitForFirstFrame = true;
            videoPlayer.playbackSpeed = Mathf.Max(0.1f, videoPlaybackSpeed);

            if (!Application.isPlaying || !autoPlayVideo)
            {
                return;
            }

            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
            }

            playRoutine = StartCoroutine(PrepareAndPlay(videoPlayer));
        }

        private IEnumerator PrepareAndPlay(VideoPlayer player)
        {
            if (player == null)
            {
                yield break;
            }

            player.Stop();
            player.Prepare();

            float startedAt = Time.realtimeSinceStartup;
            while (!player.isPrepared && Time.realtimeSinceStartup - startedAt < 5f)
            {
                yield return null;
            }

            if (player.isPrepared)
            {
                player.Play();
            }

            playRoutine = null;
        }

        private void StopVideoPlayer()
        {
            if (playRoutine != null)
            {
                StopCoroutine(playRoutine);
                playRoutine = null;
            }

            if (videoPlayer == null)
            {
                return;
            }

            if (videoPlayer.isPlaying)
            {
                videoPlayer.Stop();
            }

            videoPlayer.enabled = false;
        }

        private void EnsureCamera()
        {
            if (mainCamera != null)
            {
                return;
            }

            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        private void EnsureBindings()
        {
            EnsureCamera();
            staticImageRoot ??= FindSceneObject("StaticImageRoot");
            overlayRoot ??= FindSceneObject("OverlayRoot");
            motionRoot ??= FindSceneObject("MotionRoot");
        }

        private GameObject FindSceneObject(string name)
        {
            var child = transform.Find(name);
            if (child != null)
            {
                return child.gameObject;
            }

            return GameObject.Find(name);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active)
            {
                target.SetActive(active);
            }
        }

        private static string NormalizeVideoUrl(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            if (value.Contains("://", StringComparison.Ordinal))
            {
                return value;
            }

            return new Uri(value).AbsoluteUri;
        }
    }
}
