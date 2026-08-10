using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.State;

namespace MergeGame.Client.Bootstrap
{
    /// <summary>게스트 확보, 로그인, 토큰 저장과 네 가지 서버 상태 초기화를 순서대로 수행합니다.</summary>
    public sealed class GameBootstrapper
    {
        private readonly IMergeGameApiClient _api; private readonly ISecureTokenStore _store; private readonly IGameStateStore _gameState;
        public GameBootstrapper(IMergeGameApiClient api, ISecureTokenStore store, IGameStateStore gameState = null)
        { _api = api; _store = store; _gameState = gameState; }
        public IEnumerator Run(Action<BootstrapResult> completed)
        {
            var credential = _store.LoadGuestCredential();
            if (credential == null)
            {
                ApiResult<CreateGuestPlayerResponse> created = null;
                yield return _api.CreateGuest(value => created = value);
                if (!Succeeded(created, completed)) yield break;
                credential = new GuestCredential(created.Data.playerId, created.Data.guestToken);
                _store.SaveGuestCredential(credential); // 원문은 보안 저장 경계 밖으로 전달하지 않습니다.
            }
            ApiResult<GuestLoginResponse> login = null;
            yield return _api.LoginGuest(new GuestLoginRequest { playerId = credential.PlayerId, guestToken = credential.GuestToken }, value => login = value);
            if (!Succeeded(login, completed)) yield break;
            var tokens = login.Data;
            var session = new AuthSession(tokens.playerId, tokens.accessToken, tokens.expiresAtUtc, tokens.refreshToken, tokens.refreshTokenExpiresAtUtc);
            _store.SaveSession(session); _api.AccessToken = session.AccessToken;

            ApiResult<BoardState> board = null; ApiResult<EconomySnapshot> economy = null;
            ApiResult<QuestSnapshot> quest = null; ApiResult<SocialProfileSnapshot> social = null;
            yield return _api.InitializeBoard(value => board = value); if (!Succeeded(board, completed)) yield break;
            yield return _api.InitializeEconomy(value => economy = value); if (!Succeeded(economy, completed)) yield break;
            yield return _api.InitializeQuests(value => quest = value); if (!Succeeded(quest, completed)) yield break;
            yield return _api.InitializeSocialProfile(value => social = value); if (!Succeeded(social, completed)) yield break;
            var initial = new InitialGameState { Board = board.Data, Economy = economy.Data, Quest = quest.Data, SocialProfile = social.Data };
            _gameState?.Apply(initial); // 서버 응답 네 가지가 모두 성공한 뒤에만 하나의 초기 상태로 공개합니다.
            completed?.Invoke(new BootstrapResult { Status = BootstrapStatus.Completed, State = initial });
        }
        private static bool Succeeded<T>(ApiResult<T> result, Action<BootstrapResult> completed)
        {
            if (result != null && result.IsSuccess) return true;
            completed?.Invoke(new BootstrapResult { Status = BootstrapStatus.Failed, Error = result?.Error ?? new ApiError { Kind = ApiErrorKind.Network, Code = "no_response" } });
            return false;
        }
    }
}
