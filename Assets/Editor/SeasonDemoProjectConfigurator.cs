#if UNITY_EDITOR
using System.IO;
using SeasonDemo;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeasonDemoEditor
{
    public static class SeasonDemoProjectConfigurator
    {
        private const string ScenePath = "Assets/Scenes/SeasonDemo.unity";

        [MenuItem("Season Demo/Configure Project")]
        public static void ConfigureProject()
        {
            Directory.CreateDirectory("Assets/Scenes");

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var root = new GameObject("Season Experience Controller");
            root.AddComponent<SeasonExperienceController>();

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene(ScenePath, true)
            };

            PlayerSettings.companyName = "ByteDance";
            PlayerSettings.productName = "SeasonDemo";
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.bytedance.seasondemo");
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
            AssetDatabase.SaveAssets();
            Debug.Log("SeasonDemo project configured for Android/PICO. Main scene: " + ScenePath);
        }

        public static void BuildAndroid()
        {
            ConfigureProject();
            Directory.CreateDirectory("Builds");

            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = "Builds/SeasonDemo.apk",
                target = BuildTarget.Android,
                options = BuildOptions.None
            });

            if (report.summary.result != BuildResult.Succeeded)
            {
                throw new System.InvalidOperationException("Android build failed: " + report.summary.result);
            }

            Debug.Log("Android APK built: Builds/SeasonDemo.apk");
        }
    }
}
#endif
