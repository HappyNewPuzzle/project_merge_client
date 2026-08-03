namespace MergeGame.Client.Authentication
{
    /// <summary>모바일 구현은 Keystore/Keychain을 사용해야 하며 PlayerPrefs나 로그로 원문을 내보내면 안 됩니다.</summary>
    public interface ISecureTokenStore
    {
        GuestCredential LoadGuestCredential(); void SaveGuestCredential(GuestCredential credential);
        AuthSession LoadSession(); void SaveSession(AuthSession session); void ClearSession();
    }
}

