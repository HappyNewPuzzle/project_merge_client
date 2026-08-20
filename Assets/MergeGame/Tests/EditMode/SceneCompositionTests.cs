using MergeGame.Client.Bootstrap;
using MergeGame.Client.Presentation;
using MergeGame.Client.Api;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System.IO;

namespace MergeGame.Client.Tests.EditMode
{
    public sealed class SceneCompositionTests
    {
        [Test] public void MainScene_HasCompleteRuntimeComposition()
        {
            var scene = EditorSceneManager.OpenScene("Assets/MergeGame/Scenes/MainGame.unity", OpenSceneMode.Single);
            var root = scene.GetRootGameObjects()[0];
            Assert.That(root.GetComponent<GameClientRoot>(), Is.Not.Null);
            Assert.That(root.GetComponent<GameHudPresenter>(), Is.Not.Null);
            Assert.That(root.GetComponent<MobileSessionController>(), Is.Not.Null);
            Assert.That(root.GetComponent<SafeAreaController>(), Is.Not.Null);
            var document = root.GetComponent<UIDocument>();
            Assert.That(document.panelSettings, Is.Not.Null);
            Assert.That(document.visualTreeAsset, Is.Not.Null);
            var camera = Camera.main;
            Assert.That(camera, Is.Not.Null);
            Assert.That(camera.orthographic, Is.True);
            Assert.That(camera.clearFlags, Is.EqualTo(CameraClearFlags.SolidColor));
            Assert.That(camera.GetComponent<AudioListener>(), Is.Not.Null);
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        [Test] public void Diagnostics_RemainsBoundedDuringLongSession()
        {
            var diagnostics = new ClientDiagnostics(50);
            for (var index = 0; index < 10000; index++) diagnostics.Record(new ApiObservation(200, ApiErrorKind.None, index, index.ToString()));
            Assert.That(diagnostics.Items, Has.Count.EqualTo(50));
        }
        [Test] public void MainHud_UsesCommercialLayoutAndHidesUnavailableFeatures()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/MergeGame/UI/GameHud.uxml");
            Assert.That(template, Is.Not.Null);
            var root = template.CloneTree();
            Assert.That(root.Q<ScrollView>(className: "screen"), Is.Null, "일반 플레이 루트에는 세로 ScrollView를 사용하지 않습니다.");
            Assert.That(root.Q(className: "screen"), Is.Not.Null);
            Assert.That(root.Q("board"), Is.Not.Null);
            Assert.That(root.Q("room-background"), Is.Not.Null);
            Assert.That(root.Q("generator"), Is.Not.Null);
            Assert.That(root.Q("energy-icon"), Is.Not.Null);
            Assert.That(root.Q("coin-icon"), Is.Not.Null);
            Assert.That(root.Q("quest"), Is.Not.Null);
            Assert.That(root.Q("gem-capsule").ClassListContains("feature-placeholder"), Is.True);
            Assert.That(root.Q("mascot-root").ClassListContains("hidden"), Is.True,
                "개별 투명 Sprite가 연결되기 전에는 빈 마스코트 UI를 노출하지 않습니다.");
            Assert.That(root.Q("mascot-image"), Is.Not.Null);
            Assert.That(root.Q("mascot-speech-bubble"), Is.Not.Null);
            Assert.That(root.Q<Label>("mascot-message"), Is.Not.Null);
            Assert.That(root.Q("bottom-navigation").ClassListContains("feature-placeholder"), Is.False);
            Assert.That(root.Q<Button>("nav-home").enabledSelf, Is.True);
            Assert.That(root.Q<Button>("nav-home").ClassListContains("navigation-tab-selected"), Is.True);
            Assert.That(root.Q<Button>("nav-collection").enabledSelf, Is.False);
            Assert.That(root.Q<Button>("nav-shop").enabledSelf, Is.False);
            Assert.That(root.Q<Button>("nav-quest").enabledSelf, Is.False);
            Assert.That(root.Q("nav-home-icon"), Is.Not.Null);
            Assert.That(root.Q("development-actions").ClassListContains("development-only"), Is.True);

            var uxml = File.ReadAllText("Assets/MergeGame/UI/GameHud.uxml");
            var uss = File.ReadAllText("Assets/MergeGame/UI/GameHud.uss");
            Assert.That(uxml, Does.Not.Contain("빈 슬롯"));
            Assert.That(uss, Does.Contain(".board-item-art { width: 88%; height: 88%;"));
            Assert.That(uss, Does.Contain(".feature-placeholder { display: none; }"));

            var presenter = File.ReadAllText("Assets/MergeGame/Runtime/Presentation/GameHudPresenter.cs");
            Assert.That(presenter, Does.Not.Contain("_root?.SetEnabled"), "명령 중 Root 전체를 disabled 처리하면 화면이 깜빡입니다.");
            Assert.That(presenter, Does.Contain("EnsureBoardSlots"), "보드 슬롯 VisualElement를 재사용해야 합니다.");
            Assert.That(presenter, Does.Contain("PlayMergeSuccess"));
            Assert.That(presenter, Does.Contain("PlayGenerateSuccess"));

            var rootSource = File.ReadAllText("Assets/MergeGame/Runtime/Bootstrap/GameClientRoot.cs");
            Assert.That(rootSource, Does.Contain("result.Outcome == BoardCommandOutcome.Failed"));
            Assert.That(rootSource, Does.Contain("playSuccessFeedback?.Invoke()"), "성공 응답 적용 후에만 효과를 호출해야 합니다.");
        }
    }
}
