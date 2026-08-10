#if UNITY_ANDROID && !UNITY_EDITOR
using System;
using UnityEngine;

namespace MergeGame.Client.Authentication
{
    /// <summary>AndroidKeyStore의 비내보내기 AES 키로 암호화된 값만 SharedPreferences에 저장합니다.</summary>
    public sealed class AndroidKeystoreSecretStore : IPlatformSecretStore
    {
        private const string ClassName = "com.happynewpuzzle.mergegame.SecureSecretStore";
        private readonly AndroidJavaObject _store;
        public AndroidKeystoreSecretStore()
        {
            using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
            _store = new AndroidJavaObject(ClassName, activity);
        }
        public string Get(string key) => _store.Call<string>("get", key);
        public void Set(string key, string value) => _store.Call("set", key, value);
        public void Delete(string key) => _store.Call("delete", key);
    }
}
#endif

