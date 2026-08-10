using MergeGame.Client.Bootstrap;
using MergeGame.Client.Presentation;
using MergeGame.Client.Api;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UIElements;

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
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        }
        [Test] public void Diagnostics_RemainsBoundedDuringLongSession()
        {
            var diagnostics = new ClientDiagnostics(50);
            for (var index = 0; index < 10000; index++) diagnostics.Record(new ApiObservation(200, ApiErrorKind.None, index, index.ToString()));
            Assert.That(diagnostics.Items, Has.Count.EqualTo(50));
        }
    }
}
