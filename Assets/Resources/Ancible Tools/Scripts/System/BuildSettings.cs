#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Assets.Resources.Ancible_Tools.Scripts.System
{
    [CreateAssetMenu(fileName = "Build Settings", menuName = "Ancible Tools/Build Settings")]
    public class BuildSettings : ScriptableObject
    {
        public string DevWindowPath;
        public string DevLinuxPath;
        public string DevMacOSPath;
        public string BuildVersionFilePath;
        public int Major = 0;
        public int Build = 0;

        public string[] SceneNames;

        public void BuildDev()
        {
            var version = $"{Major}.{Build}";
            PlayerSettings.bundleVersion = version;
            File.WriteAllText(BuildVersionFilePath, version);
            if (BuildWindowsDev())
            {

                if (BuildLinuxDev())
                {
                //    BuildMacDev();
                }
            }
        }

        public bool BuildWindowsDev()
        {
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = SceneNames;
            buildPlayerOptions.locationPathName = $"{DevWindowPath}.exe";
            buildPlayerOptions.target = BuildTarget.StandaloneWindows64;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.CompressWithLz4HC | BuildOptions.ShowBuiltPlayer;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Windows Build Success! {summary.totalTime.Seconds} seconds - {summary.outputPath}");
                return true;
            }
            else
            {
                if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"Window Build Status - {summary.result}");
                }
                else
                {
                    Debug.LogWarning($"Windows Build Status - {summary.result}");
                }

                return false;
            }
        }

        public bool BuildLinuxDev()
        {
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = SceneNames;
            buildPlayerOptions.locationPathName = $"{DevLinuxPath}.x86";
            buildPlayerOptions.target = BuildTarget.StandaloneLinux64;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.CompressWithLz4HC | BuildOptions.ShowBuiltPlayer;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"Linux Build Success! {summary.totalTime.Seconds} seconds - {summary.outputPath}");
                return true;
            }
            else
            {
                if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"Linux Build Status - {summary.result}");
                }
                else
                {
                    Debug.LogWarning($"Linux Build Status - {summary.result}");
                }

                return false;
            }
        }

        public void BuildMacDev()
        {
            var buildPlayerOptions = new BuildPlayerOptions();
            buildPlayerOptions.scenes = SceneNames;
            buildPlayerOptions.locationPathName = $"{DevMacOSPath}";
            buildPlayerOptions.target = BuildTarget.StandaloneOSX;
            buildPlayerOptions.options = BuildOptions.Development | BuildOptions.CompressWithLz4HC | BuildOptions.ShowBuiltPlayer;
            buildPlayerOptions.targetGroup = BuildTargetGroup.Standalone;

            var report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"MacOS Build Success! {summary.totalTime.Seconds} seconds - {summary.outputPath}");
            }
            else
            {
                if (summary.result == BuildResult.Failed)
                {
                    Debug.LogError($"MacOS Build Status - {summary.result}");
                }
                else
                {
                    Debug.LogWarning($"MacOS Build Status - {summary.result}");
                }
            }
        }

    }
}
#endif