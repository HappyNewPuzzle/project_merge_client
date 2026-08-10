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
        private static void Build(BuildTarget target, string path)
        {
            var scenes = EditorBuildSettings.scenes.Where(value => value.enabled).Select(value => value.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("빌드 Scene이 없습니다.");
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions { scenes = scenes, target = target, locationPathName = path, options = BuildOptions.Development });
            if (report.summary.result != BuildResult.Succeeded) throw new InvalidOperationException("플레이어 빌드가 실패했습니다.");
        }
    }
}
