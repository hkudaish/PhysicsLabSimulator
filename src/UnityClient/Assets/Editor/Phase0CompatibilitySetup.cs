using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using VirtualStem.Phase0;

namespace VirtualStem.EditorTools
{
    public static class Phase0CompatibilitySetup
    {
        private const string ScenePath = "Assets/Phase0Compatibility/Scenes/CompatibilityProbe.unity";

        [MenuItem("Virtual STEM/Phase 0/Open compatibility probe")]
        public static void OpenProbe()
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            if (File.Exists(ScenePath))
            {
                EditorSceneManager.OpenScene(ScenePath);
                return;
            }
            Directory.CreateDirectory("Assets/Phase0Compatibility/Scenes");
            AssetDatabase.Refresh();
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            new GameObject("Phase 0 compatibility diagnostics").AddComponent<CompatibilityProbeView>();
            if (!EditorSceneManager.SaveScene(scene, ScenePath))
                throw new InvalidOperationException("Could not save compatibility probe scene.");
        }

        // This explicitly selects IL2CPP and restores previous project settings even after a failure.
        public static void BuildWindowsIl2CppProbe()
        {
            if (!Application.unityVersion.StartsWith("6000.3.27f2", StringComparison.Ordinal))
                throw new InvalidOperationException("Use the pinned editor 6000.3.27f2 for this evidence build.");
            if (!File.Exists(ScenePath))
                throw new InvalidOperationException("Create and review the probe through the Phase 0 menu first.");
            var target = UnityEditor.Build.NamedBuildTarget.Standalone;
            var previous = PlayerSettings.GetScriptingBackend(target);
            string output = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../../artifacts/unity-compatibility/VirtualStemCompatibility.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            try
            {
                PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.IL2CPP);
                var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = new[] { ScenePath },
                    locationPathName = output,
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.Development
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new InvalidOperationException("Compatibility probe build failed: " + report.summary.result);
                Debug.Log("IL2CPP compatibility probe: " + output);
            }
            finally { PlayerSettings.SetScriptingBackend(target, previous); }
        }
    }
}
