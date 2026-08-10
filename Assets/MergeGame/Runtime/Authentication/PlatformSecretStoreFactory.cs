using System;

namespace MergeGame.Client.Authentication
{
    /// <summary>출시 플랫폼에서는 반드시 OS 보안 저장소를 선택하며 평문 저장으로 폴백하지 않습니다.</summary>
    public static class PlatformSecretStoreFactory
    {
        public static IPlatformSecretStore Create()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            return new AndroidKeystoreSecretStore();
#elif UNITY_IOS && !UNITY_EDITOR
            return new IosKeychainSecretStore();
#else
            throw new PlatformNotSupportedException("Editor에서는 InMemoryTokenStore를 명시적으로 주입해야 합니다.");
#endif
        }
    }
}

