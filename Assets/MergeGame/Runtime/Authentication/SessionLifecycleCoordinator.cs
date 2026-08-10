using System;
using System.Collections;
using MergeGame.Client.Api;

namespace MergeGame.Client.Authentication
{
    /// <summary>앱 시작·포그라운드 복귀 때 세션을 복원하고 만료 임박 token만 선제 갱신합니다.</summary>
    public sealed class SessionLifecycleCoordinator
    {
        private static readonly TimeSpan RefreshLeadTime = TimeSpan.FromMinutes(2);
        private readonly IMergeGameApiClient _api; private readonly ISecureTokenStore _store; private readonly TokenRefreshCoordinator _refresh;
        public SessionLifecycleCoordinator(IMergeGameApiClient api, ISecureTokenStore store, TokenRefreshCoordinator refresh)
        { _api = api; _store = store; _refresh = refresh; }

        public IEnumerator RestoreOrRefresh(DateTimeOffset nowUtc, Action<ApiResult<AuthSession>> completed)
        {
            var session = _store.LoadSession();
            if (session == null)
            {
                completed?.Invoke(ApiResult<AuthSession>.Failure(ApiErrorKind.Unauthorized, 401, "session_missing"));
                yield break;
            }
            if (!DateTimeOffset.TryParse(session.AccessTokenExpiresAtUtc, out var expiresAt) || expiresAt <= nowUtc.Add(RefreshLeadTime))
            {
                yield return _refresh.Refresh(completed);
                yield break;
            }
            _api.AccessToken = session.AccessToken;
            completed?.Invoke(ApiResult<AuthSession>.Success(session));
        }
    }
}

