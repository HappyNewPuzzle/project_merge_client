using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using NUnit.Framework;
using UnityEngine.TestTools;
using MergeGame.Client.State;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.Gameplay.Progression;
using MergeGame.Client.Gameplay.Social;

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
        [UnityTest]
        public IEnumerator QuestClaim_ReusesIntentKeyAndAppliesServerReward()
        {
            var api = new DelayedApi(); var state = new GameStateStore();
            state.ApplyQuest(new QuestSnapshot { questId = "q", revision = 2 });
            state.ApplyEconomy(new EconomySnapshot { revision = 3, coins = 0 });
            var intent = QuestClaimIntent.Create("q"); var service = new ProgressionCommandService(api, state);
            ProgressionResult result = null; yield return service.ClaimQuest(intent, value => result = value);
            Assert.That(api.LastIdempotencyKey, Is.EqualTo(intent.IdempotencyKey));
            Assert.That(api.LastQuestRevision, Is.EqualTo(2));
            Assert.That(api.LastQuestEconomyRevision, Is.EqualTo(3));
            Assert.That(result.Outcome, Is.EqualTo(ProgressionOutcome.Succeeded));
            Assert.That(state.Economy.coins, Is.EqualTo(50));
        }
        [UnityTest]
        public IEnumerator DailyRewardConflict_ReloadsWithoutRepeatingRewardRequest()
        {
            var api = new DelayedApi { ConflictDailyReward = true }; var state = new GameStateStore();
            state.ApplyEconomy(new EconomySnapshot { revision = 3 }); state.ApplyQuest(new QuestSnapshot { revision = 2 });
            var service = new ProgressionCommandService(api, state); ProgressionResult result = null;
            yield return service.ClaimDailyReward(value => result = value);
            Assert.That(api.DailyRewardCalls, Is.EqualTo(1));
            Assert.That(api.GetEconomyCalls, Is.EqualTo(1));
            Assert.That(api.GetQuestCalls, Is.EqualTo(1));
            Assert.That(result.Outcome, Is.EqualTo(ProgressionOutcome.ConflictResynchronized));
        }
        [UnityTest]
        public IEnumerator AddFriend_NormalizesCodeAndReloadsServerSocialState()
        {
            var api = new DelayedApi(); var state = new GameStateStore(); var service = new SocialCommandService(api, state);
            SocialCommandResult result = null; yield return service.AddFriend(" ab12cd34 ", value => result = value);
            Assert.That(api.LastFriendCode, Is.EqualTo("AB12CD34"));
            Assert.That(api.GetSocialCalls, Is.EqualTo(1));
            Assert.That(result.State.friends, Has.Length.EqualTo(1));
            Assert.That(state.Social.friendCode, Is.EqualTo("ME123456"));
        }
        private sealed class CoroutineHost : UnityEngine.MonoBehaviour { }
        private sealed class DelayedApi : IMergeGameApiClient
        {
            public int RefreshCalls; public int BoardCalls; public int MergeCalls; public int GetBoardCalls;
            public int DailyRewardCalls; public int GetEconomyCalls; public int GetQuestCalls;
            public int GetSocialCalls; public string LastFriendCode;
            public long LastMergeRevision; public long LastGenerateBoardRevision; public long LastGenerateEconomyRevision;
            public long LastQuestRevision; public long LastQuestEconomyRevision; public string LastIdempotencyKey;
            public bool FailBoardOnce; public bool ConflictMergeOnce; public bool ConflictDailyReward; public string AccessToken { get; set; }
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
            public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> c) { GetEconomyCalls++; c(ApiResult<EconomySnapshot>.Success(new EconomySnapshot { revision = 10 })); yield break; }
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
            public IEnumerator ClaimDailyReward(RevisionRequest b, Action<ApiResult<EconomySnapshot>> c)
            {
                DailyRewardCalls++;
                c(ConflictDailyReward ? ApiResult<EconomySnapshot>.Failure(ApiErrorKind.RevisionConflict, 409) : ApiResult<EconomySnapshot>.Success(new EconomySnapshot { revision = b.expectedRevision + 1, coins = 50 }));
                yield break;
            }
            public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> c) { yield break; }
            public IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> c) { GetQuestCalls++; c(ApiResult<QuestSnapshot>.Success(new QuestSnapshot { revision = 11 })); yield break; }
            public IEnumerator ClaimQuestReward(string id, ClaimQuestRewardRequest b, Action<ApiResult<QuestRewardResponse>> c)
            {
                LastIdempotencyKey = b.idempotencyKey; LastQuestRevision = b.expectedQuestRevision; LastQuestEconomyRevision = b.expectedEconomyRevision;
                c(ApiResult<QuestRewardResponse>.Success(new QuestRewardResponse
                {
                    quest = new QuestSnapshot { questId = id, revision = b.expectedQuestRevision + 1, isClaimed = true },
                    economy = new EconomySnapshot { revision = b.expectedEconomyRevision + 1, coins = 50 }
                }));
                yield break;
            }
            public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> c) { yield break; }
            public IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> c)
            {
                GetSocialCalls++; c(ApiResult<SocialState>.Success(new SocialState { friendCode = "ME123456", friends = new[] { new FriendSnapshot { playerId = "f" } } })); yield break;
            }
            public IEnumerator AddFriend(AddFriendRequest b, Action<ApiResult<AddFriendResponse>> c)
            { LastFriendCode = b.friendCode; c(ApiResult<AddFriendResponse>.Success(new AddFriendResponse { friendPlayerId = "f" })); yield break; }
            public IEnumerator SendFriendEnergyGift(string id, Action<ApiResult<EnergyGiftResponse>> c)
            { c(ApiResult<EnergyGiftResponse>.Success(new EnergyGiftResponse { recipientEconomy = new EconomySnapshot { energy = 5 } })); yield break; }
        }
    }
}
