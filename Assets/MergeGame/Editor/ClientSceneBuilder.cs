using MergeGame.Client.Bootstrap;
using MergeGame.Client.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

namespace MergeGame.Client.Editor
{
    /// <summary>동일한 HUD Scene을 재현 가능하게 생성해 수동 Inspector 설정 누락을 방지합니다.</summary>
    public static class ClientSceneBuilder
    {
        [MenuItem("Merge Game/Build Main Client Scene")]
        public static void Build()
        {
            const string panelPath = "Assets/MergeGame/UI/GamePanelSettings.asset";
            var panel = AssetDatabase.LoadAssetAtPath<PanelSettings>(panelPath);
            if (panel == null) { panel = ScriptableObject.CreateInstance<PanelSettings>(); AssetDatabase.CreateAsset(panel, panelPath); }
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var root = new GameObject("GameClientRoot");
            var document = root.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MergeGame/UI/GameHud.uxml");
            root.AddComponent<GameHudPresenter>(); root.AddComponent<GameClientRoot>(); root.AddComponent<MobileSessionController>();
            root.AddComponent<SafeAreaController>();
            EditorSceneManager.SaveScene(scene, "Assets/MergeGame/Scenes/MainGame.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/MergeGame/Scenes/MainGame.unity", true) };
            AssetDatabase.SaveAssets();
        }
    }
}
