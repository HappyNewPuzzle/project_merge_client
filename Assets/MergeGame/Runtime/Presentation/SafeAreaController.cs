using UnityEngine;
using UnityEngine.UIElements;

namespace MergeGame.Client.Presentation
{
    /// <summary>노치와 시스템 UI 영역을 피해 HUD root의 여백을 기기 safe area에 맞춥니다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        private Rect _last;
        private void Update()
        {
            if (_last == Screen.safeArea || Screen.width <= 0 || Screen.height <= 0) return;
            _last = Screen.safeArea; var root = GetComponent<UIDocument>().rootVisualElement;
            root.style.paddingLeft = _last.xMin; root.style.paddingRight = Screen.width - _last.xMax;
            root.style.paddingBottom = _last.yMin; root.style.paddingTop = Screen.height - _last.yMax;
        }
    }
}

