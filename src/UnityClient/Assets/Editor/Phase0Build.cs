using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace VirtualStem.EditorTools
{
    public static class Phase0Build
    {
        // Empty vendor-template build only. No laboratory features are introduced.
        public static void BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            if (scenes.Length == 0)
                throw new InvalidOperationException("No enabled scaffold scene in build settings.");

            var output = Path.GetFullPath(Path.Combine(Application.dataPath,
                "../../../artifacts/unity-phase0/VirtualStemScaffold.exe"));
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = output,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new InvalidOperationException("Phase 0 Windows scaffold build failed: " + report.summary.result);
            Debug.Log("Phase 0 scaffold build: " + output);
        }
    }
}
