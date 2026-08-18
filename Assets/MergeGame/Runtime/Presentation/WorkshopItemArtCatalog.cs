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

        /// <summary>알 수 없는 체인과 유효하지 않은 레벨에는 잘못된 그림을 추측해 표시하지 않습니다.</summary>
        public Sprite Find(string chainId, int level)
        {
            var sprites = chainId == "toy" ? toySprites : chainId == "workshop" ? levelSprites : null;
            if (sprites == null || level <= 0 || level > sprites.Length) return null;
            return sprites[level - 1];
        }
    }
}
