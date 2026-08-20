using System;
using UnityEngine;

namespace MergeGame.Client.Presentation
{
    /// <summary>서버 chainId와 level을 실제 표시 Sprite에 연결하는 기존 공용 아이템 아트 카탈로그입니다.</summary>
    [CreateAssetMenu(menuName = "Merge Game/Workshop Item Art Catalog")]
    public sealed class WorkshopItemArtCatalog : ScriptableObject
    {
        [Tooltip("배열 0번이 서버 레벨 1이며 최대 9단계까지 순서대로 연결합니다.")]
        public Sprite[] levelSprites = Array.Empty<Sprite>();
        [Tooltip("Toy 체인의 Lv01~Lv08 Sprite입니다. 다음 단계 결정은 서버가 담당합니다.")]
        public Sprite[] toySprites = Array.Empty<Sprite>();
        [Tooltip("Food 아트 라인의 Lv01~Lv08 Sprite입니다. 실제 chainId 계약과 머지 결과는 서버가 담당합니다.")]
        public Sprite[] foodSprites = Array.Empty<Sprite>();
        [Tooltip("Rest 아트 라인의 Lv01~Lv08 Sprite입니다. 실제 chainId 계약과 머지 결과는 서버가 담당합니다.")]
        public Sprite[] restSprites = Array.Empty<Sprite>();
        [Tooltip("Toy Lv01~Lv08의 Presentation 전용 배율입니다. 게임 레벨이나 머지 규칙에는 사용하지 않습니다.")]
        public float[] toyVisualScales = { 1.69f, 1.18f, 1.45f, 1.25f, 1.06f, 0.82f, 1.03f, 0.90f };
        [Tooltip("Food Lv01~Lv08의 Presentation 전용 배율입니다.")]
        public float[] foodVisualScales = { 1.35f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f, 0.97f };
        [Tooltip("Rest Lv01~Lv08의 Presentation 전용 배율입니다.")]
        public float[] restVisualScales = { 0.95f, 0.97f, 0.97f, 0.98f, 0.95f, 0.97f, 0.95f, 0.97f };

        /// <summary>알 수 없는 체인과 유효하지 않은 레벨에는 잘못된 그림을 추측해 표시하지 않습니다.</summary>
        public Sprite Find(string chainId, int level)
        {
            // food/rest는 현재 서버 카탈로그에 없는 표시용 후보 키입니다. 서버가 이 키를 내려줄 때만 사용되며
            // 클라이언트가 아이템 종류나 머지 결과를 생성하는 근거로 사용하지 않습니다.
            var sprites = chainId == "toy" ? toySprites
                : chainId == "food" ? foodSprites
                : chainId == "rest" ? restSprites
                : chainId == "workshop" ? levelSprites
                : null;
            if (sprites == null || level <= 0 || level > sprites.Length) return null;
            return sprites[level - 1];
        }

        /// <summary>
        /// 서버 레벨이나 머지 규칙이 아니라 현재 Sprite의 투명 여백만 보정하는 UI 크기를 반환합니다.
        /// 알 수 없는 체인은 기존 공통 크기를 사용하므로 향후 서버 콘텐츠도 안전하게 표시됩니다.
        /// </summary>
        public float FindVisualScale(string chainId, int level)
        {
            var scales = chainId == "toy" ? toyVisualScales
                : chainId == "food" ? foodVisualScales
                : chainId == "rest" ? restVisualScales
                : null;
            return scales != null && level > 0 && level <= scales.Length && scales[level - 1] > 0f
                ? scales[level - 1]
                : 1f;
        }
    }
}
