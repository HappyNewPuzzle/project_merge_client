using System;
using System.Collections;
using MergeGame.Client.Api;

namespace MergeGame.Client.Authentication
{
    /// <summary>동시 401 요청들이 회전형 refresh token을 한 번만 제출하도록 결과를 공유합니다.</summary>
    public sealed class TokenRefreshCoordinator
    {
        private readonly IMergeGameApiClient _api; private readonly ISecureTokenStore _store;
        private bool _refreshing; private ApiResult<AuthSession> _lastResult;
        public TokenRefreshCoordinator(IMergeGameApiClient api, ISecureTokenStore store) { _api = api; _store = store; }
        public IEnumerator Refresh(Action<ApiResult<AuthSession>> completed)
        {
            if (_refreshing) { while (_refreshing) yield return null; completed?.Invoke(_lastResult); yield break; }
            _refreshing = true;
            var current = _store.LoadSession();
            if (current == null || string.IsNullOrWhiteSpace(current.RefreshToken))
            {
                _lastResult = ApiResult<AuthSession>.Failure(ApiErrorKind.Unauthorized, 401, "missing_refresh_token");
                _refreshing = false; completed?.Invoke(_lastResult); yield break;
            }
            ApiResult<GuestLoginResponse> response = null;
            yield return _api.RefreshAccessToken(new RefreshTokenRequest { refreshToken = current.RefreshToken }, value => response = value);
            if (response != null && response.IsSuccess)
            {
                var value = response.Data;
                var session = new AuthSession(value.playerId, value.accessToken, value.expiresAtUtc, value.refreshToken, value.refreshTokenExpiresAtUtc);
                _store.SaveSession(session); // 회전된 두 토큰을 한 저장 호출로 함께 교체합니다.
                _api.AccessToken = session.AccessToken; _lastResult = ApiResult<AuthSession>.Success(session);
            }
            else
            {
                if (response?.Error?.Kind == ApiErrorKind.Unauthorized) _store.ClearSession();
                _lastResult = ApiResult<AuthSession>.Failure(response?.Error?.Kind ?? ApiErrorKind.Network, response?.StatusCode ?? 0, response?.Error?.Code ?? "refresh_failed");
            }
            _refreshing = false; completed?.Invoke(_lastResult);
        }
    }
}
