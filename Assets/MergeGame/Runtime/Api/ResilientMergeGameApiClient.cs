using System;
using System.Collections;
using MergeGame.Client.Authentication;

namespace MergeGame.Client.Api
{
    /// <summary>
    /// 보호 요청의 401만 refresh 단일화 후 정확히 한 번 재시도합니다. 403 정지와 409 충돌은
    /// 사용자 판단이나 상태 재동기화가 필요하므로 변경 요청을 자동 반복하지 않습니다.
    /// </summary>
    public sealed class ResilientMergeGameApiClient : IMergeGameApiClient
    {
        private readonly IMergeGameApiClient _inner;
        private readonly TokenRefreshCoordinator _refresh;
        private readonly ISecureTokenStore _tokens;
        public ResilientMergeGameApiClient(IMergeGameApiClient inner, TokenRefreshCoordinator refresh, ISecureTokenStore tokens)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _refresh = refresh ?? throw new ArgumentNullException(nameof(refresh));
            _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
        }
        public string AccessToken { get => _inner.AccessToken; set => _inner.AccessToken = value; }

        public IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> c) => Execute(_inner.CreateGuest, false, c);
        public IEnumerator LoginGuest(GuestLoginRequest b, Action<ApiResult<GuestLoginResponse>> c) => Execute(done => _inner.LoginGuest(b, done), false, c);
        public IEnumerator RefreshAccessToken(RefreshTokenRequest b, Action<ApiResult<GuestLoginResponse>> c) => Execute(done => _inner.RefreshAccessToken(b, done), false, c);
        public IEnumerator Logout(RefreshTokenRequest b, Action<ApiResult<EmptyResponse>> c) => Execute(done => _inner.Logout(b, done), true, c);
        public IEnumerator GetCurrentPlayer(Action<ApiResult<CurrentPlayerResponse>> c) => Execute(_inner.GetCurrentPlayer, true, c);
        public IEnumerator InitializeBoard(Action<ApiResult<BoardState>> c) => Execute(_inner.InitializeBoard, true, c);
        public IEnumerator GetBoard(Action<ApiResult<BoardState>> c) => Execute(_inner.GetBoard, true, c);
        public IEnumerator MergeItems(MergeBoardItemsRequest b, Action<ApiResult<BoardState>> c) => Execute(done => _inner.MergeItems(b, done), true, c);
        public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> c) => Execute(_inner.InitializeEconomy, true, c);
        public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> c) => Execute(_inner.GetEconomy, true, c);
        public IEnumerator GenerateItem(GenerateItemRequest b, Action<ApiResult<GenerateItemResponse>> c) => Execute(done => _inner.GenerateItem(b, done), true, c);
        public IEnumerator ClaimDailyReward(RevisionRequest b, Action<ApiResult<EconomySnapshot>> c) => Execute(done => _inner.ClaimDailyReward(b, done), true, c);
        public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> c) => Execute(_inner.InitializeQuests, true, c);
        public IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> c) => Execute(_inner.GetQuests, true, c);
        public IEnumerator ClaimQuestReward(string id, ClaimQuestRewardRequest b, Action<ApiResult<QuestRewardResponse>> c) => Execute(done => _inner.ClaimQuestReward(id, b, done), true, c);
        public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> c) => Execute(_inner.InitializeSocialProfile, true, c);
        public IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> c) => Execute(_inner.GetSocialProfile, true, c);
        public IEnumerator AddFriend(AddFriendRequest b, Action<ApiResult<AddFriendResponse>> c) => Execute(done => _inner.AddFriend(b, done), true, c);
        public IEnumerator SendFriendEnergyGift(string id, Action<ApiResult<EnergyGiftResponse>> c) => Execute(done => _inner.SendFriendEnergyGift(id, done), true, c);

        private IEnumerator Execute<T>(Func<Action<ApiResult<T>>, IEnumerator> operation, bool protectedRequest, Action<ApiResult<T>> completed)
        {
            ApiResult<T> result = null;
            var retried = false;
            yield return operation(value => result = value);
            if (protectedRequest && result?.Error?.Kind == ApiErrorKind.Unauthorized)
            {
                ApiResult<AuthSession> refreshed = null;
                yield return _refresh.Refresh(value => refreshed = value);
                if (refreshed?.IsSuccess == true)
                {
                    retried = true;
                    result = null;
                    yield return operation(value => result = value); // 무한 재시도를 막기 위한 유일한 재호출입니다.
                }
                else
                {
                    completed?.Invoke(ApiResult<T>.Failure(refreshed?.Error?.Kind ?? ApiErrorKind.Unauthorized,
                        refreshed?.StatusCode ?? 401, refreshed?.Error?.Code ?? "refresh_failed"));
                    yield break;
                }
            }
            // 정지된 계정 또는 새 토큰으로도 거부된 세션은 이후 보호 요청에 재사용하지 않습니다.
            if (result?.Error?.Kind == ApiErrorKind.AccountSuspended || (retried && result?.Error?.Kind == ApiErrorKind.Unauthorized))
                _tokens.ClearSession();
            completed?.Invoke(result);
        }
    }
}
