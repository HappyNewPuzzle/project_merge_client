using System;
using MergeGame.Client.Gameplay.Board;
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

namespace MergeGame.Client.Presentation
{
    /// <summary>UI Toolkit 문서에 서버 상태 모델을 표시하고 사용자 명령 의도를 콜백으로 전달합니다.</summary>
    [RequireComponent(typeof(UIDocument))]
    public sealed class GameHudPresenter : MonoBehaviour
    {
        private VisualElement _board; private Label _status; private Label _economy; private Label _quest; private Label _friendCode;
        private readonly Dictionary<int, VisualElement> _slotElements = new();
        private BoardSlotView _dragSource; private VisualElement _dragElement; private Label _dragGhost;
        private int _pointerId = -1; private Vector2 _pointerStart; private bool _dragging;
        private WorkshopItemArtCatalog _itemArt;
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
            _itemArt = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            _board = root.Q("board"); _status = root.Q<Label>("status"); _economy = root.Q<Label>("economy");
            _quest = root.Q<Label>("quest"); _friendCode = root.Q<Label>("friend-code");
            root.Q<Button>("daily")?.RegisterCallback<ClickEvent>(_ => DailyRewardRequested?.Invoke());
            root.Q<Button>("claim")?.RegisterCallback<ClickEvent>(_ => QuestClaimRequested?.Invoke());
            root.Q<Button>("add-friend")?.RegisterCallback<ClickEvent>(_ => AddFriendRequested?.Invoke(root.Q<TextField>("friend-input")?.value ?? ""));
            root.Q<Button>("retry")?.RegisterCallback<ClickEvent>(_ => RetryRequested?.Invoke());
            root.Q<Button>("logout")?.RegisterCallback<ClickEvent>(_ => LogoutRequested?.Invoke());
            _board.RegisterCallback<PointerDownEvent>(OnPointerDown);
            _board.RegisterCallback<PointerMoveEvent>(OnPointerMove);
            _board.RegisterCallback<PointerUpEvent>(OnPointerUp);
            _board.RegisterCallback<PointerCancelEvent>(OnPointerCancel);
        }
        public void SetInteractionEnabled(bool enabled) => _root?.SetEnabled(enabled);
        public void Render(GameUiModel model)
        {
            _status.text = model.Message; _economy.text = $"에너지 {model.Energy} · 코인 {model.Coins}";
            _quest.text = "퀘스트 " + model.QuestText; _friendCode.text = "친구 코드 " + model.FriendCode;
            CancelDrag(); _board.Clear(); _slotElements.Clear();
            foreach (var slot in model.Slots)
            {
                var element = new VisualElement { userData = slot,
                    tooltip = slot.IsEmpty ? $"{slot.SlotIndex}번 빈 슬롯, 누르면 아이템 생성" : $"{slot.SlotIndex}번 {slot.Name}, 레벨 {slot.Level}. 같은 아이템으로 드래그해 머지" };
                element.AddToClassList("board-slot"); element.AddToClassList(slot.IsEmpty ? "board-slot-empty" : "board-slot-item");
                var sprite = _itemArt?.Find(slot.ChainId, slot.Level);
                if (sprite != null)
                {
                    var art = new VisualElement { pickingMode = PickingMode.Ignore };
                    art.style.backgroundImage = new StyleBackground(sprite); art.AddToClassList("board-item-art"); element.Add(art);
                }
                element.Add(new Label(slot.IsEmpty ? $"{slot.SlotIndex}\n{KoreanStrings.EmptySlot}" : $"{slot.SlotIndex}\n{slot.Name}\nLv.{slot.Level}"));
                _slotElements[slot.SlotIndex] = element; _board.Add(element);
            }
            var friends = _root.Q("friends"); friends?.Clear();
            foreach (var friend in model.Friends)
            {
                var id = friend.playerId;
                var button = new Button(() => EnergyGiftRequested?.Invoke(id)) { text = friend.displayName + (friend.energyGiftSentToday ? " · 선물 완료" : " · 에너지 선물") };
                button.SetEnabled(!friend.energyGiftSentToday); friends?.Add(button);
            }
        }
        private void OnPointerDown(PointerDownEvent evt)
        {
            var element = FindSlot(evt.target as VisualElement);
            if (element == null || element.userData is not BoardSlotView slot) return;
            _pointerId = evt.pointerId; _pointerStart = evt.position; _dragSource = slot; _dragElement = element;
            _board.CapturePointer(evt.pointerId); evt.StopPropagation();
        }
        private void OnPointerMove(PointerMoveEvent evt)
        {
            if (evt.pointerId != _pointerId || _dragSource.IsEmpty) return;
            if (!_dragging && Vector2.Distance(_pointerStart, evt.position) < 8f) return;
            if (!_dragging) BeginDrag();
            var local = _board.WorldToLocal(evt.position); _dragGhost.style.left = local.x - 38; _dragGhost.style.top = local.y - 34;
            HighlightTargets(); evt.StopPropagation();
        }
        private void OnPointerUp(PointerUpEvent evt)
        {
            if (evt.pointerId != _pointerId) return;
            var targetElement = FindSlot(_board.panel?.Pick(evt.position));
            var hasTarget = targetElement?.userData is BoardSlotView;
            var target = hasTarget ? (BoardSlotView)targetElement.userData : default;
            if (_dragging && hasTarget && BoardMergeRules.CanMerge(_dragSource, target))
                MergeRequested?.Invoke(_dragSource.SlotIndex, target.SlotIndex);
            else if (!_dragging && _dragSource.IsEmpty)
                GenerateRequested?.Invoke(_dragSource.SlotIndex);
            CancelDrag(); evt.StopPropagation();
        }
        private void OnPointerCancel(PointerCancelEvent evt) { if (evt.pointerId == _pointerId) CancelDrag(); }
        private void BeginDrag()
        {
            _dragging = true; _dragElement.AddToClassList("board-slot-dragging");
            _dragGhost = new Label($"{_dragSource.Name}\nLv.{_dragSource.Level}") { pickingMode = PickingMode.Ignore };
            var sprite = _itemArt?.Find(_dragSource.ChainId, _dragSource.Level);
            if (sprite != null) _dragGhost.style.backgroundImage = new StyleBackground(sprite);
            _dragGhost.AddToClassList("drag-ghost"); _board.Add(_dragGhost);
        }
        private void HighlightTargets()
        {
            foreach (var pair in _slotElements)
            {
                var slot = (BoardSlotView)pair.Value.userData;
                pair.Value.EnableInClassList("board-slot-merge-target", BoardMergeRules.CanMerge(_dragSource, slot));
            }
        }
        private void CancelDrag()
        {
            if (_pointerId >= 0 && _board != null && _board.HasPointerCapture(_pointerId)) _board.ReleasePointer(_pointerId);
            _dragElement?.RemoveFromClassList("board-slot-dragging"); _dragGhost?.RemoveFromHierarchy();
            foreach (var element in _slotElements.Values) element.RemoveFromClassList("board-slot-merge-target");
            _pointerId = -1; _dragging = false; _dragElement = null; _dragGhost = null; _dragSource = default;
        }
        private static VisualElement FindSlot(VisualElement element)
        {
            while (element != null && element.userData is not BoardSlotView) element = element.parent;
            return element;
        }
    }
}
