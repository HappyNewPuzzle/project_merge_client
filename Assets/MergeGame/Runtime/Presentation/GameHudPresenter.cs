using System;
using MergeGame.Client.Gameplay.Board;
using UnityEngine;
using UnityEngine.UIElements;

namespace MergeGame.Client.Presentation
{
    /// <summary>UI Toolkit 문서에 서버 상태 모델을 표시하고 사용자 명령 의도를 콜백으로 전달합니다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameHudPresenter : MonoBehaviour
    {
        private VisualElement _board; private Label _status; private Label _economy; private Label _quest; private Label _friendCode;
        private int _firstSelection = -1;
        private VisualElement _root;
        public event Action<int> GenerateRequested;
        public event Action<int, int> MergeRequested;
        public event Action DailyRewardRequested;
        public event Action QuestClaimRequested;
        public event Action<string> AddFriendRequested;
        public event Action<string> EnergyGiftRequested;
        public event Action RetryRequested;
        public event Action LogoutRequested;

        private void Awake()
        {
            var root = GetComponent<UIDocument>().rootVisualElement; _root = root;
            _board = root.Q("board"); _status = root.Q<Label>("status"); _economy = root.Q<Label>("economy");
            _quest = root.Q<Label>("quest"); _friendCode = root.Q<Label>("friend-code");
            root.Q<Button>("daily")?.RegisterCallback<ClickEvent>(_ => DailyRewardRequested?.Invoke());
            root.Q<Button>("claim")?.RegisterCallback<ClickEvent>(_ => QuestClaimRequested?.Invoke());
            root.Q<Button>("add-friend")?.RegisterCallback<ClickEvent>(_ => AddFriendRequested?.Invoke(root.Q<TextField>("friend-input")?.value ?? ""));
            root.Q<Button>("retry")?.RegisterCallback<ClickEvent>(_ => RetryRequested?.Invoke());
            root.Q<Button>("logout")?.RegisterCallback<ClickEvent>(_ => LogoutRequested?.Invoke());
        }
        public void SetInteractionEnabled(bool enabled) => _root?.SetEnabled(enabled);
        public void Render(GameUiModel model)
        {
            _status.text = model.Message; _economy.text = $"에너지 {model.Energy} · 코인 {model.Coins}";
            _quest.text = "퀘스트 " + model.QuestText; _friendCode.text = "친구 코드 " + model.FriendCode;
            _board.Clear();
            foreach (var slot in model.Slots)
            {
                var button = new Button(() => Select(slot))
                {
                    text = slot.IsEmpty ? $"{slot.SlotIndex}\n{KoreanStrings.EmptySlot}" : $"{slot.SlotIndex}\n{slot.Name} Lv.{slot.Level}",
                    tooltip = slot.IsEmpty ? $"{slot.SlotIndex}번 빈 슬롯, 선택하면 아이템 생성" : $"{slot.SlotIndex}번 {slot.Name}, 레벨 {slot.Level}"
                };
                button.AddToClassList("board-slot"); _board.Add(button);
            }
            var friends = _root.Q("friends"); friends?.Clear();
            foreach (var friend in model.Friends)
            {
                var id = friend.playerId;
                var button = new Button(() => EnergyGiftRequested?.Invoke(id)) { text = friend.displayName + (friend.energyGiftSentToday ? " · 선물 완료" : " · 에너지 선물") };
                button.SetEnabled(!friend.energyGiftSentToday); friends?.Add(button);
            }
        }
        private void Select(BoardSlotView slot)
        {
            if (slot.IsEmpty) { _firstSelection = -1; GenerateRequested?.Invoke(slot.SlotIndex); return; }
            if (_firstSelection < 0) { _firstSelection = slot.SlotIndex; return; }
            var source = _firstSelection; _firstSelection = -1;
            if (source != slot.SlotIndex) MergeRequested?.Invoke(source, slot.SlotIndex);
        }
    }
}
