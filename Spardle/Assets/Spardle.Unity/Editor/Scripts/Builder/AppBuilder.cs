using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

namespace Spardle.Unity.Editor.Scripts.Builder
{
    public class AppBuilder
    {
        private const string IosAppIdentifier = "com.danpany.spardle.development";
        private const string AndroidAppIdentifier = "com.danpany.spardle.development";
        private const string BuildDirPath = "Builds";

        [PostProcessBuild]
        public static void OnPostProcessBuild(BuildTarget buildTarget, string path)
        {
            if (buildTarget == BuildTarget.iOS)
            {
                _processForIos(path);
            }
        }

        private static void _processForIos(string path)
        {
            var pbx = new PBXProject();
            var pbxPath = PBXProject.GetPBXProjectPath(path);
            pbx.ReadFromString(File.ReadAllText(pbxPath));
            var target = pbx.GetUnityFrameworkTargetGuid();
            pbx.SetBuildProperty(target, "ENABLE_BITCODE", "NO");
            File.WriteAllText(pbxPath, pbx.WriteToString());
        }

        private static string[] _getAllScenePaths()
        {
            return EditorBuildSettings.scenes.Select(scene => scene.path).ToArray();
        }

        private static string _getBuildDirectory()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), BuildDirPath);
        }

        private static void _build(string profileName)
        {
            var profileSettings = AddressableAssetSettingsDefaultObject.Settings.profileSettings;
            var profileId = profileSettings.GetProfileId(profileName);
            AddressableAssetSettingsDefaultObject.Settings.activeProfileId = profileId;
            AddressableAssetSettings.BuildPlayerContent();
        }

        [MenuItem("Tools/Build/AddressableAsset")]
        public static void BuildAddressableAsset()
        {
            _build("Default");
        }

        [MenuItem("Tools/Build/iOS")]
        public static void BuildIosApp()
        {
#if !UNITY_IOS
            throw new InvalidOperationException();
#endif

            PlayerSettings.applicationIdentifier = IosAppIdentifier;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.iOS, ScriptingImplementation.IL2CPP);
            var buildOptions = BuildOptions.None;
            var errorMessage = BuildPipeline.BuildPlayer(_getAllScenePaths(),
                Path.Combine(_getBuildDirectory(), "Spardle"), BuildTarget.iOS, buildOptions);
        }

        [MenuItem("Tools/Build/Android")]
        public static void BuildAndroidApp()
        {
#if !UNITY_ANDROID
            throw new InvalidOperationException();
#endif

            var androidBundleVersionCode = Environment.GetEnvironmentVariable("ANDROID_BUNDLE_VERSION_CODE");
            var keystorePassword = Environment.GetEnvironmentVariable("ANDROID_KEYSTORE_PASSWORD");
            if (androidBundleVersionCode != null)
            {
                PlayerSettings.Android.bundleVersionCode = int.Parse(androidBundleVersionCode);
            }
            PlayerSettings.Android.keystorePass = keystorePassword;
            PlayerSettings.Android.keyaliasPass = keystorePassword;
            PlayerSettings.applicationIdentifier = AndroidAppIdentifier;
            PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
            EditorUserBuildSettings.buildAppBundle = true;
            var buildOptions = BuildOptions.None;
            var buildReport = BuildPipeline.BuildPlayer(_getAllScenePaths(),
                Path.Combine(_getBuildDirectory(), "Spardle.aab"), BuildTarget.Android, buildOptions);
        }
    }
}
