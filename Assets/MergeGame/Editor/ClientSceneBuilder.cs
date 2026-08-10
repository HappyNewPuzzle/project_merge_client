using MergeGame.Client.Bootstrap;
using MergeGame.Client.Presentation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using System.IO;

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
            var serializedDocument = new SerializedObject(document);
            serializedDocument.FindProperty("m_PanelSettings").objectReferenceValue = panel;
            serializedDocument.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(document);
            root.AddComponent<GameHudPresenter>(); root.AddComponent<GameClientRoot>(); root.AddComponent<MobileSessionController>();
            root.AddComponent<SafeAreaController>();
            document.panelSettings = panel;
            serializedDocument.Update();
            serializedDocument.FindProperty("m_PanelSettings").objectReferenceValue = panel;
            serializedDocument.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(document);
            EditorSceneManager.SaveScene(scene, "Assets/MergeGame/Scenes/MainGame.unity");
            // Unity 6.3 batchmode가 UIDocument PanelSettings를 0으로 저장하는 경우 정확한 한 필드만 보정합니다.
            var sceneText = File.ReadAllText("Assets/MergeGame/Scenes/MainGame.unity");
            var panelGuid = AssetDatabase.AssetPathToGUID(panelPath);
            sceneText = sceneText.Replace("m_PanelSettings: {fileID: 0}", $"m_PanelSettings: {{fileID: 11400000, guid: {panelGuid}, type: 2}}");
            File.WriteAllText("Assets/MergeGame/Scenes/MainGame.unity", sceneText);
            AssetDatabase.ImportAsset("Assets/MergeGame/Scenes/MainGame.unity", ImportAssetOptions.ForceUpdate);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene("Assets/MergeGame/Scenes/MainGame.unity", true) };
            AssetDatabase.SaveAssets();
        }
    }
}
