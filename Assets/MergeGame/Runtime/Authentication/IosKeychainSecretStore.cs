#if UNITY_IOS && !UNITY_EDITOR
using System;
using System.Runtime.InteropServices;

namespace MergeGame.Client.Authentication
{
    /// <summary>iOS Keychain의 ThisDeviceOnly 항목을 사용하는 네이티브 어댑터입니다.</summary>
    public sealed class IosKeychainSecretStore : IPlatformSecretStore
    {
        [DllImport("__Internal")] private static extern IntPtr MergeGameKeychainGet(string key);
        [DllImport("__Internal")] private static extern bool MergeGameKeychainSet(string key, string value);
        [DllImport("__Internal")] private static extern void MergeGameKeychainDelete(string key);
        [DllImport("__Internal")] private static extern void MergeGameFreeString(IntPtr value);
        public string Get(string key)
        {
            var pointer = MergeGameKeychainGet(key);
            if (pointer == IntPtr.Zero) return null;
            try { return Marshal.PtrToStringAnsi(pointer); }
            finally { MergeGameFreeString(pointer); }
        }
        public void Set(string key, string value)
        {
            if (!MergeGameKeychainSet(key, value)) throw new InvalidOperationException("iOS Keychain 저장에 실패했습니다.");
        }
        public void Delete(string key) => MergeGameKeychainDelete(key);
    }
}
#endif
