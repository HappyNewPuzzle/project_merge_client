using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using NUnit.Framework;
using UnityEngine.TestTools;
using MergeGame.Client.State;

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
        private sealed class CoroutineHost : UnityEngine.MonoBehaviour { }
        private sealed class DelayedApi : IMergeGameApiClient
        {
            public int RefreshCalls; public int BoardCalls; public bool FailBoardOnce; public string AccessToken { get; set; }
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
            public IEnumerator GetBoard(Action<ApiResult<BoardState>> c) { yield break; }
            public IEnumerator MergeItems(MergeBoardItemsRequest b, Action<ApiResult<BoardState>> c) { yield break; }
            public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> c) { yield break; }
            public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> c) { yield break; }
            public IEnumerator GenerateItem(GenerateItemRequest b, Action<ApiResult<GenerateItemResponse>> c) { yield break; }
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
