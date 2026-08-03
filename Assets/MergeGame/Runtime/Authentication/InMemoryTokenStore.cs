namespace MergeGame.Client.Authentication
{
    /// <summary>Editor 개발과 테스트 전용입니다. 출시 빌드에서는 플랫폼 보안 저장 구현을 주입해야 합니다.</summary>
    public sealed class InMemoryTokenStore : ISecureTokenStore
    {
        private GuestCredential _credential; private AuthSession _session;
        public GuestCredential LoadGuestCredential() => _credential;
        public void SaveGuestCredential(GuestCredential credential) => _credential = credential;
        public AuthSession LoadSession() => _session;
        public void SaveSession(AuthSession session) => _session = session;
        public void ClearSession() => _session = null;
    }
}

