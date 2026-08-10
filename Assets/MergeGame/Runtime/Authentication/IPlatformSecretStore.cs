namespace MergeGame.Client.Authentication
{
    /// <summary>운영체제 보안 저장소의 최소 기능입니다. 값이 없을 때 null을 반환합니다.</summary>
    public interface IPlatformSecretStore
    {
        string Get(string key);
        void Set(string key, string value);
        void Delete(string key);
    }
}

