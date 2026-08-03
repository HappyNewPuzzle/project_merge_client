using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using NUnit.Framework;
using UnityEngine.TestTools;

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
        private sealed class CoroutineHost : UnityEngine.MonoBehaviour { }
        private sealed class DelayedApi : IMergeGameApiClient
        {
            public int RefreshCalls; public string AccessToken { get; set; }
            public IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> done)
            {
                RefreshCalls++; yield return null;
                done(ApiResult<GuestLoginResponse>.Success(new GuestLoginResponse { playerId = "p", accessToken = "new-a", refreshToken = "new-r" }));
            }
            public IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> c) { yield break; }
            public IEnumerator LoginGuest(GuestLoginRequest b, Action<ApiResult<GuestLoginResponse>> c) { yield break; }
            public IEnumerator InitializeBoard(Action<ApiResult<BoardState>> c) { yield break; }
            public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> c) { yield break; }
            public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> c) { yield break; }
            public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> c) { yield break; }
        }
    }
}
