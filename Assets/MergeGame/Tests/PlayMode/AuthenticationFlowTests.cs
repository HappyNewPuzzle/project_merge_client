using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using NUnit.Framework;
using UnityEngine.TestTools;
using MergeGame.Client.State;
using MergeGame.Client.Gameplay.Board;

namespace MergeGame.Client.Tests.PlayMode
{
    public sealed class AuthenticationFlowTests
    {
        [UnityTest]
        public IEnumerator ConcurrentRefresh_UsesServerOnceAndSharesResult()
        {
            var api = new DelayedApi(); var store = new InMemoryTokenStore();
            store.SaveSession(new AuthSession("p", "old-a", "", "old-r", ""));
            var coordinator = new TokenRefreshCoordinator(api, store);
            ApiResult<AuthSession> first = null, second = null;
            var host = new UnityEngine.GameObject("refresh-test").AddComponent<CoroutineHost>();
            host.StartCoroutine(coordinator.Refresh(value => first = value));
            host.StartCoroutine(coordinator.Refresh(value => second = value));
            while (first == null || second == null) yield return null;
            Assert.That(api.RefreshCalls, Is.EqualTo(1));
            Assert.That(first.Data.AccessToken, Is.EqualTo("new-a"));
            Assert.That(second.Data.RefreshToken, Is.EqualTo("new-r"));
            UnityEngine.Object.Destroy(host.gameObject);
        }
        [UnityTest]
        public IEnumerator Protected401_RefreshesAndRetriesOriginalRequestOnce()
        {
            var api = new DelayedApi { FailBoardOnce = true }; var store = new InMemoryTokenStore();
            store.SaveSession(new AuthSession("p", "old-a", "", "old-r", ""));
            var resilient = new ResilientMergeGameApiClient(api, new TokenRefreshCoordinator(api, store), store);
            ApiResult<BoardState> result = null;
            yield return resilient.InitializeBoard(value => result = value);
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(api.RefreshCalls, Is.EqualTo(1));
            Assert.That(api.BoardCalls, Is.EqualTo(2));
        }
        [UnityTest]
        public IEnumerator RevisionConflict_ReloadsButNeverRepeatsMutation()
        {
            var resolver = new RevisionConflictResolver(); var mutationCalls = 0; var reloadCalls = 0;
            MutationResult<BoardState, BoardState> result = null;
            IEnumerator Mutate(Action<ApiResult<BoardState>> done)
            {
                mutationCalls++;
                done(ApiResult<BoardState>.Failure(ApiErrorKind.RevisionConflict, 409, "stale_revision"));
                yield break;
            }
            IEnumerator Reload(Action<ApiResult<BoardState>> done)
            {
                reloadCalls++; done(ApiResult<BoardState>.Success(new BoardState { revision = 7 })); yield break;
            }
            yield return resolver.Execute<BoardState, BoardState>(Mutate, Reload, value => result = value);
            Assert.That(mutationCalls, Is.EqualTo(1));
            Assert.That(reloadCalls, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(MutationOutcome.ConflictResynchronized));
            Assert.That(result.LatestServerState.revision, Is.EqualTo(7));
        }
        [UnityTest]
        public IEnumerator BoardMerge_UsesStoredRevisionAndAppliesOnlyServerResponse()
        {
            var api = new DelayedApi(); var state = new GameStateStore();
            state.ApplyBoard(new BoardState { width = 5, height = 7, revision = 4 });
            var service = new BoardCommandService(api, state); BoardCommandResult result = null;
            yield return service.Merge(1, 2, value => result = value);
            Assert.That(api.LastMergeRevision, Is.EqualTo(4));
            Assert.That(result.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(state.Board.revision, Is.EqualTo(5));
        }
        [UnityTest]
        public IEnumerator BoardMergeConflict_ReloadsStateWithoutRepeatingMerge()
        {
            var api = new DelayedApi { ConflictMergeOnce = true }; var state = new GameStateStore();
            state.ApplyBoard(new BoardState { revision = 4 }); state.ApplyEconomy(new EconomySnapshot { revision = 2 });
            var service = new BoardCommandService(api, state); BoardCommandResult result = null;
            yield return service.Merge(1, 2, value => result = value);
            Assert.That(api.MergeCalls, Is.EqualTo(1));
            Assert.That(api.GetBoardCalls, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(BoardCommandOutcome.ConflictResynchronized));
            Assert.That(state.Board.revision, Is.EqualTo(9));
        }
        [UnityTest]
        public IEnumerator Generate_UsesBothServerRevisionsAndAppliesReturnedSnapshots()
        {
            var api = new DelayedApi(); var state = new GameStateStore();
            state.ApplyBoard(new BoardState { revision = 6 }); state.ApplyEconomy(new EconomySnapshot { revision = 8, energy = 10 });
            var service = new BoardCommandService(api, state); BoardCommandResult result = null;
            yield return service.Generate(3, value => result = value);
            Assert.That(api.LastGenerateBoardRevision, Is.EqualTo(6));
            Assert.That(api.LastGenerateEconomyRevision, Is.EqualTo(8));
            Assert.That(result.Outcome, Is.EqualTo(BoardCommandOutcome.Succeeded));
            Assert.That(state.Economy.energy, Is.EqualTo(9));
        }
        private sealed class CoroutineHost : UnityEngine.MonoBehaviour { }
        private sealed class DelayedApi : IMergeGameApiClient
        {
            public int RefreshCalls; public int BoardCalls; public int MergeCalls; public int GetBoardCalls;
            public long LastMergeRevision; public long LastGenerateBoardRevision; public long LastGenerateEconomyRevision;
            public bool FailBoardOnce; public bool ConflictMergeOnce; public string AccessToken { get; set; }
            public IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> done)
            {
                RefreshCalls++; yield return null;
                done(ApiResult<GuestLoginResponse>.Success(new GuestLoginResponse { playerId = "p", accessToken = "new-a", refreshToken = "new-r" }));
            }
            public IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> c) { yield break; }
            public IEnumerator LoginGuest(GuestLoginRequest b, Action<ApiResult<GuestLoginResponse>> c) { yield break; }
            public IEnumerator Logout(RefreshTokenRequest b, Action<ApiResult<EmptyResponse>> c) { yield break; }
            public IEnumerator GetCurrentPlayer(Action<ApiResult<CurrentPlayerResponse>> c) { yield break; }
            public IEnumerator InitializeBoard(Action<ApiResult<BoardState>> c)
            {
                BoardCalls++;
                if (FailBoardOnce && BoardCalls == 1) c(ApiResult<BoardState>.Failure(ApiErrorKind.Unauthorized, 401));
                else c(ApiResult<BoardState>.Success(new BoardState()));
                yield break;
            }
            public IEnumerator GetBoard(Action<ApiResult<BoardState>> c) { GetBoardCalls++; c(ApiResult<BoardState>.Success(new BoardState { revision = 9 })); yield break; }
            public IEnumerator MergeItems(MergeBoardItemsRequest b, Action<ApiResult<BoardState>> c)
            {
                MergeCalls++; LastMergeRevision = b.expectedRevision;
                c(ConflictMergeOnce ? ApiResult<BoardState>.Failure(ApiErrorKind.RevisionConflict, 409, "stale_revision") : ApiResult<BoardState>.Success(new BoardState { revision = b.expectedRevision + 1 }));
                yield break;
            }
            public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> c) { yield break; }
            public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> c) { c(ApiResult<EconomySnapshot>.Success(new EconomySnapshot { revision = 3 })); yield break; }
            public IEnumerator GenerateItem(GenerateItemRequest b, Action<ApiResult<GenerateItemResponse>> c)
            {
                LastGenerateBoardRevision = b.expectedBoardRevision; LastGenerateEconomyRevision = b.expectedEconomyRevision;
                c(ApiResult<GenerateItemResponse>.Success(new GenerateItemResponse
                {
                    board = new BoardState { revision = b.expectedBoardRevision + 1 },
                    economy = new EconomySnapshot { revision = b.expectedEconomyRevision + 1, energy = 9 }
                }));
                yield break;
            }
            public IEnumerator ClaimDailyReward(RevisionRequest b, Action<ApiResult<EconomySnapshot>> c) { yield break; }
            public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> c) { yield break; }
            public IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> c) { yield break; }
            public IEnumerator ClaimQuestReward(string id, ClaimQuestRewardRequest b, Action<ApiResult<QuestRewardResponse>> c) { yield break; }
            public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> c) { yield break; }
            public IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> c) { yield break; }
            public IEnumerator AddFriend(AddFriendRequest b, Action<ApiResult<AddFriendResponse>> c) { yield break; }
            public IEnumerator SendFriendEnergyGift(string id, Action<ApiResult<EnergyGiftResponse>> c) { yield break; }
        }
    }
}
