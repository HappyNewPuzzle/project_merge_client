using System;
using MergeGame.Client.Api;

namespace MergeGame.Client.Gameplay.Board
{
    public readonly struct BoardSlotView
    {
        public int SlotIndex { get; }
        public bool IsEmpty { get; }
        public string ItemId { get; }
        public string ChainId { get; }
        public string Name { get; }
        public int Level { get; }
        public bool IsMaxLevel { get; }
        public BoardSlotView(int slotIndex, BoardItemState item)
        {
            SlotIndex = slotIndex; IsEmpty = item == null; ItemId = item?.itemId ?? "";
            ChainId = item?.chainId ?? ""; Name = item?.name ?? ""; Level = item?.level ?? 0; IsMaxLevel = item?.isMaxLevel ?? false;
        }
    }

    /// <summary>드롭 가능 여부를 화면 추측이 아니라 서버가 준 체인·레벨·최대 레벨 정보로 판단합니다.</summary>
    public static class BoardMergeRules
    {
        public static bool CanMerge(BoardSlotView source, BoardSlotView target) =>
            !source.IsEmpty && !target.IsEmpty && source.SlotIndex != target.SlotIndex &&
            !source.IsMaxLevel && source.Level == target.Level &&
            !string.IsNullOrWhiteSpace(source.ChainId) && source.ChainId == target.ChainId;
    }

    /// <summary>서버 보드 스냅샷을 UI가 바로 그릴 수 있는 고정 슬롯 배열로 투영합니다.</summary>
    public static class BoardPresentationState
    {
        public static BoardSlotView[] Create(BoardState board)
        {
            if (board == null) return Array.Empty<BoardSlotView>();
            var count = checked(board.width * board.height);
            var indexed = new BoardItemState[count];
            foreach (var item in board.items ?? Array.Empty<BoardItemState>())
                if (item != null && item.slotIndex >= 0 && item.slotIndex < count) indexed[item.slotIndex] = item;
            var slots = new BoardSlotView[count];
            for (var index = 0; index < count; index++) slots[index] = new BoardSlotView(index, indexed[index]);
            return slots;
        }
    }
}
