using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.Build;

namespace MergeGame.Client.Editor
{
    /// <summary>CI와 로컬에서 동일한 Scene 목록으로 재현 가능한 플레이어 빌드를 생성합니다.</summary>
    public static class ClientBuild
    {
        public static void BuildWindowsDevelopment() => Build(BuildTarget.StandaloneWindows64, "Builds/Windows/ProjectMerge.exe");
        public static void BuildAndroidDevelopment()
        {
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.happynewpuzzle.projectmerge");
            Build(BuildTarget.Android, "Builds/Android/ProjectMerge.apk");
        }
        public static void BuildAndroidRelease()
        {
            RejectOfflineMockForRelease(NamedBuildTarget.Android);
            PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android, "com.happynewpuzzle.projectmerge");
            Build(BuildTarget.Android, "Builds/Android/ProjectMerge.aab", BuildOptions.None);
        }
        /// <summary>서명 비밀값을 저장소에 기록하지 않고 CI 환경에서만 주입해 Google Play용 AAB를 만듭니다.</summary>
        public static void BuildAndroidStoreRelease()
        {
            var keystore = RequireEnvironment("MERGEGAME_ANDROID_KEYSTORE_PATH");
            var keystorePass = RequireEnvironment("MERGEGAME_ANDROID_KEYSTORE_PASSWORD");
            var alias = RequireEnvironment("MERGEGAME_ANDROID_KEY_ALIAS");
            var aliasPass = RequireEnvironment("MERGEGAME_ANDROID_KEY_PASSWORD");
            var previous = (PlayerSettings.Android.keystoreName, PlayerSettings.Android.keystorePass,
                PlayerSettings.Android.keyaliasName, PlayerSettings.Android.keyaliasPass);
            try
            {
                PlayerSettings.Android.keystoreName = keystore;
                PlayerSettings.Android.keystorePass = keystorePass;
                PlayerSettings.Android.keyaliasName = alias;
                PlayerSettings.Android.keyaliasPass = aliasPass;
                BuildAndroidRelease();
            }
            finally
            {
                // 장시간 실행되는 Editor와 로컬 ProjectSettings에 비밀값이 잔류하지 않도록 원래 값을 복원합니다.
                PlayerSettings.Android.keystoreName = previous.Item1;
                PlayerSettings.Android.keystorePass = previous.Item2;
                PlayerSettings.Android.keyaliasName = previous.Item3;
                PlayerSettings.Android.keyaliasPass = previous.Item4;
            }
        }
        public static void BuildIosXcode() => Build(BuildTarget.iOS, "Builds/iOS");
        private static void RejectOfflineMockForRelease(NamedBuildTarget target)
        {
            var defines = PlayerSettings.GetScriptingDefineSymbols(target);
            if (defines.Split(';').Contains("MERGEGAME_OFFLINE_MOCK"))
                throw new InvalidOperationException("배포 빌드에는 MERGEGAME_OFFLINE_MOCK define을 포함할 수 없습니다.");
        }
        private static string RequireEnvironment(string name) =>
            Environment.GetEnvironmentVariable(name) is { Length: > 0 } value
                ? value
                : throw new InvalidOperationException($"필수 환경 변수가 없습니다: {name}");
        private static void Build(BuildTarget target, string path, BuildOptions options = BuildOptions.Development)
        {
            var scenes = EditorBuildSettings.scenes.Where(value => value.enabled).Select(value => value.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("빌드 Scene이 없습니다.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, target = target, locationPathName = path, options = options });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("플레이어 빌드가 실패했습니다.");
        }
    }
}
