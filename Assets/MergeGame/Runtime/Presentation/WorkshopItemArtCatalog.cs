using System;
using UnityEngine;

namespace MergeGame.Client.Presentation
{
    /// <summary>워크숍 체인의 서버 레벨을 아틀라스에서 분할된 표시 Sprite에 연결하는 공개 자산입니다.</summary>
    [CreateAssetMenu(menuName = "Merge Game/Workshop Item Art Catalog")]
    public sealed class WorkshopItemArtCatalog : ScriptableObject
    {
        [Tooltip("배열 0번이 서버 레벨 1이며 최대 9단계까지 순서대로 연결합니다.")]
        public Sprite[] levelSprites = Array.Empty<Sprite>();

        /// <summary>알 수 없는 체인과 유효하지 않은 레벨에는 잘못된 그림을 추측해 표시하지 않습니다.</summary>
        public Sprite Find(string chainId, int level)
        {
            if (chainId != "workshop" || level <= 0 || level > levelSprites.Length) return null;
            return levelSprites[level - 1];
        }
    }
}
