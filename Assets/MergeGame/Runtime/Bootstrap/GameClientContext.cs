using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.Configuration;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.Gameplay.Progression;
using MergeGame.Client.Gameplay.Social;
using MergeGame.Client.State;

namespace MergeGame.Client.Bootstrap
{
    /// <summary>하나의 앱 수명 동안 공유할 인증·API·게임 상태 객체를 일관되게 조립합니다.</summary>
    public sealed class GameClientContext
    {
        public IMergeGameApiClient Api { get; internal set; }
        public ISecureTokenStore Tokens { get; internal set; }
        public IGameStateStore State { get; internal set; }
        public GameBootstrapper Bootstrapper { get; internal set; }
        public SessionLifecycleCoordinator SessionLifecycle { get; internal set; }
        public BoardCommandService Board { get; internal set; }
        public ProgressionCommandService Progression { get; internal set; }
        public SocialCommandService Social { get; internal set; }
    }

    public static class GameClientContextFactory
    {
        public static GameClientContext CreateForPlayer()
        {
            var tokens = new SecureTokenStore(PlatformSecretStoreFactory.Create());
            return Create(ServerEndpointCatalog.Current.BaseUrl, tokens);
        }
        public static GameClientContext Create(string baseUrl, ISecureTokenStore tokens)
        {
            var raw = new MergeGameApiClient(baseUrl);
            var refresh = new TokenRefreshCoordinator(raw, tokens);
            var api = new ResilientMergeGameApiClient(raw, refresh, tokens);
            var state = new GameStateStore();
            return new GameClientContext
            {
                Api = api, Tokens = tokens, State = state,
                Bootstrapper = new GameBootstrapper(api, tokens, state),
                SessionLifecycle = new SessionLifecycleCoordinator(api, tokens, refresh),
                Board = new BoardCommandService(api, state),
                Progression = new ProgressionCommandService(api, state),
                Social = new SocialCommandService(api, state)
            };
        }
    }
}

