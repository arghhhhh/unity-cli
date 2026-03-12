using System;
using UnityEditor;
using UnityEngine;

namespace UnityCliBridge.TestScenes
{
    public static class UnityCliInputBatchHost
    {
        private static bool _initialized;
        private static string _shutdownFilePath;

        public static void Run()
        {
            if (_initialized)
            {
                return;
            }

            _shutdownFilePath = Environment.GetEnvironmentVariable("UNITY_CLI_BATCH_HOST_SHUTDOWN_FILE");
            if (string.IsNullOrWhiteSpace(_shutdownFilePath))
            {
                var port = Environment.GetEnvironmentVariable("UNITY_CLI_PORT_OVERRIDE") ??
                           Environment.GetEnvironmentVariable("UNITY_CLI_PORT") ??
                           "6402";
                _shutdownFilePath = $"/tmp/unity-cli-batch-host-stop-{port}";
            }

            _initialized = true;
            EditorApplication.update -= Tick;
            EditorApplication.update += Tick;
            Debug.Log($"Unity CLI batch host keep-alive started. Shutdown file: {_shutdownFilePath}");
        }

        private static void Tick()
        {
            if (!_initialized)
            {
                return;
            }

            if (!string.IsNullOrEmpty(_shutdownFilePath) && System.IO.File.Exists(_shutdownFilePath))
            {
                Debug.Log($"Unity CLI batch host shutdown requested via {_shutdownFilePath}");
                EditorApplication.update -= Tick;
                _initialized = false;
                EditorApplication.Exit(0);
            }
        }
    }
}
