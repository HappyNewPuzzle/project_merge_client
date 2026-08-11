using UnityEngine;
using UnityEngine.UIElements;

namespace MergeGame.Client.Presentation
{
    /// <summary>화면 픽셀 기준 safe area를 UI Toolkit panel 단위로 변환한 네 방향 여백입니다.</summary>
    public readonly struct SafeAreaInsets
    {
        public float Left { get; } public float Top { get; } public float Right { get; } public float Bottom { get; }
        public SafeAreaInsets(float left, float top, float right, float bottom)
        { Left = left; Top = top; Right = right; Bottom = bottom; }
    }

    /// <summary>노치와 시스템 UI 영역을 피해 HUD root의 여백을 기기 safe area에 맞춥니다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class SafeAreaController : MonoBehaviour
    {
        private Rect _last;
        private void Update()
        {
            var root = GetComponent<UIDocument>().rootVisualElement;
            if (_last == Screen.safeArea || Screen.width <= 0 || Screen.height <= 0 || root.resolvedStyle.width <= 0) return;
            _last = Screen.safeArea;
            var insets = CalculateInsets(_last, Screen.width, Screen.height, root.resolvedStyle.width, root.resolvedStyle.height);
            root.style.paddingLeft = insets.Left; root.style.paddingTop = insets.Top;
            root.style.paddingRight = insets.Right; root.style.paddingBottom = insets.Bottom;
        }

        /// <summary>노치 픽셀을 panel 스케일로 환산해 해상도 배율에 따른 과도한 여백을 방지합니다.</summary>
        public static SafeAreaInsets CalculateInsets(Rect safe, float screenWidth, float screenHeight, float panelWidth, float panelHeight)
        {
            if (screenWidth <= 0 || screenHeight <= 0 || panelWidth <= 0 || panelHeight <= 0) return new SafeAreaInsets();
            var scaleX = panelWidth / screenWidth; var scaleY = panelHeight / screenHeight;
            return new SafeAreaInsets(safe.xMin * scaleX, (screenHeight - safe.yMax) * scaleY,
                (screenWidth - safe.xMax) * scaleX, safe.yMin * scaleY);
        }
    }
}
