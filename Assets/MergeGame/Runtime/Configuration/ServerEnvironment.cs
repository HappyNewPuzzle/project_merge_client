namespace MergeGame.Client.Configuration
{
    using UnityEngine;
    using System;
    /// <summary>빌드 대상에 따라 공개 서버 주소만 선택합니다. 비밀값은 이 구조에 넣지 않습니다.</summary>
    public enum ServerEnvironment { Development, Staging, Production }
    public readonly struct ServerEndpoint
    {
        public ServerEnvironment Environment { get; }
        public string BaseUrl { get; }
        public ServerEndpoint(ServerEnvironment environment, string baseUrl) { Environment = environment; BaseUrl = baseUrl; }
    }
    public static class ServerEndpointCatalog
    {
        public static ServerEndpoint For(ServerEnvironment environment) => environment switch
        {
            ServerEnvironment.Development => new ServerEndpoint(environment, "https://localhost:7001"),
            ServerEnvironment.Staging => new ServerEndpoint(environment, "https://staging-api.example.invalid"),
            ServerEnvironment.Production => new ServerEndpoint(environment, "https://api.example.invalid"),
            _ => throw new System.ArgumentOutOfRangeException(nameof(environment))
        };
        // 실제 배포 주소는 CI에서 공개 설정으로 교체합니다. 토큰이나 관리자 키를 여기에 넣지 않습니다.
        public static ServerEndpoint Current
        {
            get
            {
                var generated = Resources.Load<TextAsset>("PublicServerConfiguration");
                if (generated != null)
                {
                    var value = JsonUtility.FromJson<GeneratedServerConfiguration>(generated.text);
                    if (value == null || !Uri.TryCreate(value.baseUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                        throw new InvalidOperationException("빌드에 포함된 공개 서버 주소가 유효한 HTTPS 주소가 아닙니다.");
                    return new ServerEndpoint(value.environment == "Production" ? ServerEnvironment.Production : ServerEnvironment.Staging, value.baseUrl);
                }
#if MERGEGAME_PRODUCTION
                return For(ServerEnvironment.Production);
#elif MERGEGAME_STAGING
                return For(ServerEnvironment.Staging);
#else
                return For(ServerEnvironment.Development);
#endif
            }
        }
        [Serializable] private sealed class GeneratedServerConfiguration { public string environment = ""; public string baseUrl = ""; }
    }
}
