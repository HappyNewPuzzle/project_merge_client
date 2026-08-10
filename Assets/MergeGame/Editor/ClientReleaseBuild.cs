using System;
using UnityEditor;
using UnityEditor.Build;

namespace MergeGame.Client.Editor
{
    /// <summary>릴리스 버전과 Android 서명값을 CI 환경에서만 받아 저장소 비밀 유출을 방지합니다.</summary>
    public static class ClientReleaseBuild
    {
        public static void BuildAndroidBundle()
        {
            ApplyVersion();
            var keystore = Required("MERGEGAME_ANDROID_KEYSTORE_PATH");
            PlayerSettings.Android.useCustomKeystore = true;
            PlayerSettings.Android.keystoreName = keystore;
            PlayerSettings.Android.keystorePass = Required("MERGEGAME_ANDROID_KEYSTORE_PASSWORD");
            PlayerSettings.Android.keyaliasName = Required("MERGEGAME_ANDROID_KEY_ALIAS");
            PlayerSettings.Android.keyaliasPass = Required("MERGEGAME_ANDROID_KEY_PASSWORD");
            EditorUserBuildSettings.buildAppBundle = true;
            ClientBuild.BuildAndroidRelease();
        }
        public static void ValidateReleaseConfiguration() { ApplyVersion(); Required("MERGEGAME_PRODUCTION_BASE_URL"); }
        private static void ApplyVersion()
        {
            var version = Required("MERGEGAME_RELEASE_VERSION");
            PlayerSettings.bundleVersion = version;
            if (!int.TryParse(Environment.GetEnvironmentVariable("MERGEGAME_ANDROID_VERSION_CODE"), out var code) || code < 1)
                throw new InvalidOperationException("Android version code가 필요합니다.");
            PlayerSettings.Android.bundleVersionCode = code;
        }
        private static string Required(string name) => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name))
            ? throw new InvalidOperationException(name + " 환경 변수가 필요합니다.") : Environment.GetEnvironmentVariable(name);
    }
}

