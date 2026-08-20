using UnityEngine;

namespace MergeGame.Client.Presentation
{
    /// <summary>
    /// 머지 아이템이 아닌 HUD 전용 그림만 보관합니다.
    /// 이 자산은 표시 책임만 가지며 생성기 종류, 재화 잔액 또는 서버 게임 규칙을 정의하지 않습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Merge Game/Workshop HUD Art Catalog")]
    public sealed class WorkshopHudArtCatalog : ScriptableObject
    {
        [Header("Generators")]
        [Tooltip("현재 서버 생성 흐름에서 실제 사용하는 Toy 생성기 그림입니다.")]
        public Sprite toyGenerator;
        [Tooltip("향후 서버 계약이 추가될 때 사용할 Food 생성기 그림입니다. 현재 생성 기능에는 사용하지 않습니다.")]
        public Sprite foodGenerator;
        [Tooltip("향후 서버 계약이 추가될 때 사용할 Rest 생성기 그림입니다. 현재 생성 기능에는 사용하지 않습니다.")]
        public Sprite restGenerator;

        [Header("Currency")]
        [Tooltip("서버 coins 값을 표시할 때 사용하는 그림입니다.")]
        public Sprite coin;
        [Tooltip("서버 energy 값을 표시할 때 사용하는 그림입니다.")]
        public Sprite energy;
        [Tooltip("향후 서버 계약이 추가될 때 사용할 Gem 그림입니다. 현재 잔액 UI에는 사용하지 않습니다.")]
        public Sprite gem;

        [Header("Mascot")]
        [Tooltip("메인 HUD에서 사용하는 투명 배경의 기본 고양이 마스코트입니다.")]
        public Sprite defaultMascot;

        [Header("Background")]
        [Tooltip("메인 플레이 화면 가장 뒤에서만 표시되는 고양이 방 배경입니다.")]
        public Sprite roomBackground;

        [Header("Bottom Navigation")]
        public Sprite navHome;
        public Sprite navCollection;
        public Sprite navShop;
        public Sprite navQuest;
    }
}
