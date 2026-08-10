using System;
using System.Collections;
using MergeGame.Client.Api;
using NUnit.Framework;
using UnityEngine.TestTools;

namespace MergeGame.Client.Tests.PlayMode
{
    /// <summary>명시적인 개발 서버 주소가 있을 때만 실행되는 실제 `/api/v1` smoke test입니다.</summary>
    public sealed class DevelopmentServerIntegrationTests
    {
        [UnityTest]
        public IEnumerator GuestLoginAndInitialState_SucceedsAgainstConfiguredServer()
        {
            var baseUrl = Environment.GetEnvironmentVariable("MERGEGAME_INTEGRATION_BASE_URL");
            if (string.IsNullOrWhiteSpace(baseUrl)) Assert.Ignore("통합 서버 주소가 없어 안전하게 건너뜁니다.");
            var api = new MergeGameApiClient(baseUrl);
            ApiResult<CreateGuestPlayerResponse> guest = null;
            yield return api.CreateGuest(value => guest = value);
            Assert.That(guest?.IsSuccess, Is.True, guest?.Error?.TraceId);
            ApiResult<GuestLoginResponse> login = null;
            yield return api.LoginGuest(new GuestLoginRequest { playerId = guest.Data.playerId, guestToken = guest.Data.guestToken }, value => login = value);
            Assert.That(login?.IsSuccess, Is.True, login?.Error?.TraceId);
            api.AccessToken = login.Data.accessToken;
            ApiResult<BoardState> board = null; ApiResult<EconomySnapshot> economy = null;
            ApiResult<QuestSnapshot> quest = null; ApiResult<SocialProfileSnapshot> social = null;
            yield return api.InitializeBoard(value => board = value);
            yield return api.InitializeEconomy(value => economy = value);
            yield return api.InitializeQuests(value => quest = value);
            yield return api.InitializeSocialProfile(value => social = value);
            Assert.That(board?.IsSuccess == true && economy?.IsSuccess == true
                && quest?.IsSuccess == true && social?.IsSuccess == true, Is.True);
        }
    }
}
