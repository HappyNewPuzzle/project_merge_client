using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Bootstrap;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.Gameplay.Progression;
using NUnit.Framework;
using UnityEngine.TestTools;
using MergeGame.Client.Presentation;
using UnityEngine;

namespace MergeGame.Client.Tests.PlayMode
{
    /// <summary>외부 서버나 socket 없이 실제 Bootstrap·명령 계층을 끝까지 통과시키는 계약 회귀 테스트입니다.</summary>
    public sealed class OfflineMockFlowTests
    {
        [UnityTest]
        public IEnumerator BootstrapGenerateAndMerge_WorkWithoutServer()
        {
            var context = GameClientContextFactory.CreateOffline();
            BootstrapResult bootstrap = null;
            yield return context.Bootstrapper.Run(value => bootstrap = value);
            Assert.That(bootstrap.IsCompleted, Is.True);
            Assert.That(context.State.Board.revision, Is.EqualTo(1));

            BoardCommandResult first = null, second = null, merge = null;
            yield return context.Board.Generate(0, value => first = value);
            yield return context.Board.Generate(1, value => second = value);
            yield return context.Board.Merge(0, 1, value => merge = value);
            ProgressionResult progression = null;
            yield return context.Progression.Reload(value => progression = value);

            Assert.That(first.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(second.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(merge.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(context.State.Board.items, Has.Length.EqualTo(1));
            Assert.That(context.State.Board.items[0].level, Is.EqualTo(2));
            Assert.That(context.State.Board.items[0].chainId, Is.EqualTo("toy"));
            Assert.That(Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog").Find("toy", 2).name, Is.EqualTo("Toy_Lv02"));
            Assert.That(context.State.Economy.energy, Is.EqualTo(8));
            Assert.That(progression.Outcome, Is.EqualTo(ProgressionOutcome.Succeeded));
            Assert.That(context.State.Quest.isCompleted, Is.True);
        }

        [UnityTest]
        public IEnumerator ToyLevelSevenMerge_ProducesServerMarkedFinalLevelEight()
        {
            var server = new MockServerState();
            server.Board.items = new[]
            {
                new BoardItemState { itemId = "toy-a", slotIndex = 0, chainId = "toy", level = 7, name = "Toy Lv.07" },
                new BoardItemState { itemId = "toy-b", slotIndex = 1, chainId = "toy", level = 7, name = "Toy Lv.07" }
            };
            var context = GameClientContextFactory.CreateOffline(new MockMergeGameApiClient(server));
            yield return context.Bootstrapper.Run(_ => { });
            BoardCommandResult result = null;
            yield return context.Board.Merge(0, 1, value => result = value);
            Assert.That(result.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(result.Board.items[0].level, Is.EqualTo(8));
            Assert.That(result.Board.items[0].isMaxLevel, Is.True);
        }

        [UnityTest]
        public IEnumerator InjectedSuspension_IsClassifiedWithoutNetwork()
        {
            var mock = new MockMergeGameApiClient { NextScenario = MockApiScenario.AccountSuspended, LatencyFrames = 1 };
            var context = GameClientContextFactory.CreateOffline(mock);
            BootstrapResult result = null;
            yield return context.Bootstrapper.Run(value => result = value);
            Assert.That(result.IsCompleted, Is.False);
            Assert.That(result.Error.Kind, Is.EqualTo(ApiErrorKind.AccountSuspended));
            Assert.That(mock.NextScenario, Is.EqualTo(MockApiScenario.Success));
        }

        [UnityTest]
        public IEnumerator ReturnedSnapshot_CannotMutateMockServerAuthority()
        {
            var mock = new MockMergeGameApiClient();
            BoardState first = null, second = null;
            yield return mock.GetBoard(value => first = value.Data);
            first.revision = 999;
            yield return mock.GetBoard(value => second = value.Data);
            Assert.That(second.revision, Is.EqualTo(1));
        }
    }
}
