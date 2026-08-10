using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using UnityEngine;

namespace MergeGame.Client.Bootstrap
{
    /// <summary>모바일 포그라운드 복귀를 세션 복원과 연결합니다. 토큰 값은 로그하지 않습니다.</summary>
    public sealed class MobileSessionController : MonoBehaviour
    {
        private SessionLifecycleCoordinator _lifecycle; private bool _checking;
        public event Action<ApiResult<AuthSession>> SessionChecked;
        public void Initialize(SessionLifecycleCoordinator lifecycle) => _lifecycle = lifecycle;
        private void OnApplicationFocus(bool hasFocus) { if (hasFocus) StartCheck(); }
        private void OnApplicationPause(bool paused) { if (!paused) StartCheck(); }
        private void StartCheck()
        {
            if (_lifecycle == null || _checking) return;
            StartCoroutine(Check());
        }
        private IEnumerator Check()
        {
            _checking = true; ApiResult<AuthSession> result = null;
            yield return _lifecycle.RestoreOrRefresh(DateTimeOffset.UtcNow, value => result = value);
            _checking = false; SessionChecked?.Invoke(result);
        }
    }
}
