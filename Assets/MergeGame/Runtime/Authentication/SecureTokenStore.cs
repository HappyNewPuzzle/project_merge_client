using System;
using UnityEngine;

namespace MergeGame.Client.Authentication
{
    /// <summary>민감 DTO를 OS 보안 저장소에만 직렬화하며 PlayerPrefs에는 기록하지 않습니다.</summary>
    public sealed class SecureTokenStore : ISecureTokenStore
    {
        private const string GuestKey = "mergegame.guest.v1";
        private const string SessionKey = "mergegame.session.v1";
        private readonly IPlatformSecretStore _secrets;
        public SecureTokenStore(IPlatformSecretStore secrets) => _secrets = secrets ?? throw new ArgumentNullException(nameof(secrets));

        public GuestCredential LoadGuestCredential()
        {
            var value = Deserialize<GuestData>(_secrets.Get(GuestKey));
            return value == null ? null : new GuestCredential(value.playerId, value.guestToken);
        }
        public void SaveGuestCredential(GuestCredential credential)
        {
            if (credential == null) throw new ArgumentNullException(nameof(credential));
            _secrets.Set(GuestKey, JsonUtility.ToJson(new GuestData { playerId = credential.PlayerId, guestToken = credential.GuestToken }));
        }
        public AuthSession LoadSession()
        {
            var value = Deserialize<SessionData>(_secrets.Get(SessionKey));
            return value == null ? null : new AuthSession(value.playerId, value.accessToken, value.accessExpiresAtUtc, value.refreshToken, value.refreshExpiresAtUtc);
        }
        public void SaveSession(AuthSession session)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            _secrets.Set(SessionKey, JsonUtility.ToJson(new SessionData
            {
                playerId = session.PlayerId, accessToken = session.AccessToken, accessExpiresAtUtc = session.AccessTokenExpiresAtUtc,
                refreshToken = session.RefreshToken, refreshExpiresAtUtc = session.RefreshTokenExpiresAtUtc
            }));
        }
        public void ClearSession() => _secrets.Delete(SessionKey);

        private static T Deserialize<T>(string json) where T : class => string.IsNullOrWhiteSpace(json) ? null : JsonUtility.FromJson<T>(json);
        [Serializable] private sealed class GuestData { public string playerId = ""; public string guestToken = ""; }
        [Serializable] private sealed class SessionData
        {
            public string playerId = ""; public string accessToken = ""; public string accessExpiresAtUtc = "";
            public string refreshToken = ""; public string refreshExpiresAtUtc = "";
        }
    }
}

