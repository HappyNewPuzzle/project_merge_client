using System;
using System.Collections;

namespace MergeGame.Client.Api
{
    /// <summary>Bootstrap과 테스트가 실제 전송 구현에 결합되지 않도록 공개 API 경계를 정의합니다.</summary>
    public interface IMergeGameApiClient
    {
        string AccessToken { get; set; }
        IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> completed);
        IEnumerator LoginGuest(GuestLoginRequest body, Action<ApiResult<GuestLoginResponse>> completed);
        IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> completed);
        IEnumerator InitializeBoard(Action<ApiResult<BoardState>> completed);
        IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> completed);
        IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> completed);
        IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> completed);
    }
}

