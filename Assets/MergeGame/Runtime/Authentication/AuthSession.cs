namespace MergeGame.Client.Authentication
{
    /// <summary>메모리에만 유지되는 현재 인증 상태입니다. 토큰은 ToString이나 로그로 출력하지 않습니다.</summary>
    public sealed class AuthSession
    {
        public string PlayerId { get; } public string AccessToken { get; } public string AccessTokenExpiresAtUtc { get; }
        public string RefreshToken { get; } public string RefreshTokenExpiresAtUtc { get; }
        public AuthSession(string playerId, string accessToken, string accessExpires, string refreshToken, string refreshExpires)
        { PlayerId = playerId; AccessToken = accessToken; AccessTokenExpiresAtUtc = accessExpires; RefreshToken = refreshToken; RefreshTokenExpiresAtUtc = refreshExpires; }
    }
    /// <summary>서버가 최초 한 번만 보여주는 게스트 로그인 자격 증명입니다.</summary>
    public sealed class GuestCredential
    {
        public string PlayerId { get; } public string GuestToken { get; }
        public GuestCredential(string playerId, string guestToken) { PlayerId = playerId; GuestToken = guestToken; }
    }
}

