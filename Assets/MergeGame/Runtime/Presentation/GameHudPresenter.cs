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
        public const int SuccessFeedbackDurationMs = 320;
        public const int InvalidDropFeedbackDurationMs = 160;
        [SerializeField, Tooltip("개발 중 슬롯 번호·아이템 이름·레벨을 확인할 때만 켭니다. 일반 플레이 기본값은 OFF입니다.")]
        private bool showBoardDebugLabels;
        private VisualElement _board; private Label _status; private Label _energy; private Label _coins; private Label _quest; private Label _friendCode; private Button _generator;
        private readonly Dictionary<int, VisualElement> _slotElements = new();
        private BoardSlotView _dragSource; private VisualElement _dragElement; private VisualElement _dragGhost;
        private int _pointerId = -1; private Vector2 _pointerStart; private bool _dragging;
        private WorkshopItemArtCatalog _itemArt;
        private WorkshopHudArtCatalog _hudArt;
        private VisualElement _root;
        private VisualElement _screen;
        private VisualElement _boardFrame;
        private VisualElement _mascotRoot;
        private VisualElement _mascotImage;
        private Label _mascotMessage;
        private VisualElement _bottomNavigation;
        private float _viewportHeight;
        private bool _mascotRequested;
        private bool _inputBlocked;
        private bool _generatorAvailable;
        public event Action GenerateRequested;
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
            _screen = root.Q(className: "screen");
            _itemArt = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            _hudArt = Resources.Load<WorkshopHudArtCatalog>("WorkshopHudArtCatalog");
            root.EnableInClassList("show-board-debug", showBoardDebugLabels);
            _board = root.Q("board"); _status = root.Q<Label>("status");
            _boardFrame = root.Q(className: "board-frame");
            _mascotRoot = root.Q("mascot-root");
            _mascotImage = root.Q("mascot-image");
            _mascotMessage = root.Q<Label>("mascot-message");
            _bottomNavigation = root.Q("bottom-navigation");
            _energy = root.Q<Label>("energy-value"); _coins = root.Q<Label>("coin-value");
            _quest = root.Q<Label>("quest"); _friendCode = root.Q<Label>("friend-code");
            _generator = root.Q<Button>("generator");
            ApplyHudArt(root);
            // UI Toolkit USS에는 일반적인 CSS media query가 없으므로 실제 Panel 높이로 compact class와
            // 보드 셀 높이를 계산합니다. 화면 폭뿐 아니라 세로 길이가 짧은 기기에서도 4×4 보드가 우선 보입니다.
            root.RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
            _generator?.RegisterCallback<ClickEvent>(_ =>
            {
                _generator.AddToClassList("generator-producing");
                _generator.schedule.Execute(() =>
                {
                    _generator.RemoveFromClassList("generator-producing");
                    _generator.AddToClassList("generator-rebound");
                    _generator.schedule.Execute(() => _generator.RemoveFromClassList("generator-rebound")).StartingIn(110);
                }).StartingIn(140);
                GenerateRequested?.Invoke();
            });
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
        /// <summary>
        /// 네트워크 명령 중에는 입력 의도만 차단합니다. Root 전체를 disabled 상태로 바꾸면 UI Toolkit이
        /// 모든 패널을 다시 tint해 화면 전체가 깜빡이므로 시각 상태는 그대로 유지합니다.
        /// </summary>
        public void SetInteractionEnabled(bool enabled)
        {
            _inputBlocked = !enabled;
            if (_inputBlocked) CancelDrag();
            UpdateGeneratorEnabled();
        }
        public void Render(GameUiModel model)
        {
            _status.text = model.Message;
            _root.Q("status-panel")?.EnableInClassList("hidden", model.Phase == GameUiPhase.Ready);
            _energy.text = $"{model.Energy} / {model.MaxEnergy}";
            _coins.text = model.Coins.ToString("N0");
            _quest.text = "퀘스트 " + model.QuestText; _friendCode.text = "친구 코드 " + model.FriendCode;
            _generatorAvailable = model.Phase == GameUiPhase.Ready && model.Energy > 0 && BoardGeneratorPlacement.FindFirstEmpty(model.Slots) >= 0;
            UpdateGeneratorEnabled();
            CancelDrag();
            var rebuilt = EnsureBoardSlots(model.Slots);
            foreach (var slot in model.Slots)
            {
                var element = _slotElements[slot.SlotIndex];
                element.userData = slot;
                element.tooltip = slot.IsEmpty ? $"{slot.SlotIndex}번 빈 슬롯" : $"{slot.SlotIndex}번 {slot.Name}, 레벨 {slot.Level}. 같은 아이템으로 드래그해 머지";
                element.EnableInClassList("board-slot-empty", slot.IsEmpty);
                element.EnableInClassList("board-slot-item", !slot.IsEmpty);
                var art = element.Q("item-art");
                var sprite = _itemArt?.Find(slot.ChainId, slot.Level);
                if (sprite != null)
                {
                    art.style.backgroundImage = new StyleBackground(sprite);
                    var visualScale = _itemArt.FindVisualScale(slot.ChainId, slot.Level);
                    art.style.width = Length.Percent(88f); art.style.height = Length.Percent(88f);
                    ApplyAnimationScale(art, visualScale, 1f);
                    art.RemoveFromClassList("hidden");
                }
                else
                {
                    art.style.backgroundImage = StyleKeyword.None;
                    art.AddToClassList("hidden");
                }
                element.Q<Label>("debug-label").text = slot.IsEmpty ? $"#{slot.SlotIndex}" : $"#{slot.SlotIndex} {slot.Name} Lv.{slot.Level}";
            }
            if (rebuilt)
            {
                ApplyResponsiveBoardHeight();
                _root.schedule.Execute(FitBoardInsideViewport).StartingIn(1);
            }
            var friends = _root.Q("friends"); friends?.Clear();
            foreach (var friend in model.Friends)
            {
                var id = friend.playerId;
                var button = new Button(() => EnergyGiftRequested?.Invoke(id)) { text = friend.displayName + (friend.energyGiftSentToday ? " · 선물 완료" : " · 에너지 선물") };
                button.SetEnabled(!friend.energyGiftSentToday); friends?.Add(button);
            }
        }
        private bool EnsureBoardSlots(BoardSlotView[] slots)
        {
            if (_slotElements.Count == slots.Length) return false;
            _board.Clear(); _slotElements.Clear();
            foreach (var slot in slots)
            {
                var element = new VisualElement { name = $"board-slot-{slot.SlotIndex}" };
                element.AddToClassList("board-slot");
                var art = new VisualElement { name = "item-art", pickingMode = PickingMode.Ignore };
                art.style.flexShrink = 0f;
                art.AddToClassList("board-item-art"); element.Add(art);
                var debugLabel = new Label { name = "debug-label", pickingMode = PickingMode.Ignore };
                debugLabel.AddToClassList("board-debug-label"); element.Add(debugLabel);
                _slotElements[slot.SlotIndex] = element; _board.Add(element);
            }
            return true;
        }

        private void UpdateGeneratorEnabled() => _generator?.SetEnabled(!_inputBlocked && _generatorAvailable);
        private void OnGeometryChanged(GeometryChangedEvent evt)
        {
            _viewportHeight = evt.newRect.height;
            _root.EnableInClassList("compact-height", _viewportHeight > 0f && _viewportHeight < 980f);
            _root.EnableInClassList("very-compact-height", _viewportHeight > 0f && _viewportHeight < 760f);
            UpdateMascotVisibility();
            ApplyResponsiveMascotSize();
            ApplyResponsiveBoardHeight();
            _root.schedule.Execute(FitBoardInsideViewport).StartingIn(1);
        }

        private void ApplyResponsiveBoardHeight()
        {
            if (_viewportHeight <= 0f) return;
            var height = CalculateBoardSlotHeight(_viewportHeight);
            foreach (var element in _slotElements.Values) element.style.height = height;
        }

        private void FitBoardInsideViewport()
        {
            if (_screen == null || _boardFrame == null || _slotElements.Count == 0) return;
            var viewportHeight = _screen.contentRect.height;
            var boardTop = _boardFrame.ChangeCoordinatesTo(_screen, Vector2.zero).y;
            var frameStyle = _boardFrame.resolvedStyle;
            var frameExtras = frameStyle.paddingTop + frameStyle.paddingBottom + frameStyle.borderTopWidth + frameStyle.borderBottomWidth;
            var first = default(VisualElement);
            foreach (var element in _slotElements.Values) { first = element; break; }
            if (first == null || float.IsNaN(viewportHeight) || float.IsNaN(boardTop)) return;
            var slotStyle = first.resolvedStyle;
            var cellMargins = slotStyle.marginTop + slotStyle.marginBottom;
            var mascotReserve = _mascotRoot != null && !_mascotRoot.ClassListContains("hidden")
                ? CalculateMascotSize(_screen.contentRect.width) + 10f
                : 0f;
            var navigationReserve = CalculateNavigationReserve(_bottomNavigation);
            var fittedHeight = CalculatePortraitBoardSlotHeight(viewportHeight, boardTop, frameExtras,
                cellMargins, _board.contentRect.width, mascotReserve + navigationReserve);
            foreach (var element in _slotElements.Values) element.style.height = fittedHeight;
        }

        /// <summary>
        /// 보드 실제 폭으로 정사각형 Cell 높이를 구하되 마스코트를 포함한 남은 세로 공간을 넘지 않습니다.
        /// Item VisualScale과 Animation Scale은 Item 하위 요소에만 적용되므로 이 계산과 독립적입니다.
        /// </summary>
        public static float CalculatePortraitBoardSlotHeight(float viewportHeight, float boardTop, float frameExtras,
            float cellMargins, float boardContentWidth, float bottomReserve)
        {
            var squareHeight = (boardContentWidth / 4f) - cellMargins;
            var availableHeight = ((viewportHeight - boardTop - frameExtras - bottomReserve - 4f) / 4f) - cellMargins;
            return Mathf.Clamp(Mathf.Min(squareHeight, availableHeight), 48f, 170f);
        }

        /// <summary>Board 시작점부터 화면 하단까지의 실제 공간을 정확히 네 행에 배분합니다.</summary>
        public static float CalculateFittedBoardSlotHeight(float viewportHeight, float boardTop, float frameExtras, float cellMargins)
        {
            var availableForRows = viewportHeight - boardTop - frameExtras - 4f; // 하단 안전 여백
            return Mathf.Clamp((availableForRows / 4f) - cellMargins, 48f, 112f);
        }

        /// <summary>
        /// 상단 HUD·Quest·Generator가 사용할 공간을 남기고 4개 보드 행의 높이를 계산합니다.
        /// 최소값은 작은 화면에서 터치 가능 크기를 지키고, 최대값은 큰 화면에서 보드가 과도하게 커지는 것을 막습니다.
        /// </summary>
        public static float CalculateBoardSlotHeight(float viewportHeight)
        {
            var reservedHeight = viewportHeight < 760f ? 330f : viewportHeight < 980f ? 350f : 410f;
            return Mathf.Clamp((viewportHeight - reservedHeight) / 4.2f, 62f, 112f);
        }

        /// <summary>Portrait에서 표정이 읽히도록 Panel 폭에 비례시키되 Board보다 커지지 않게 제한합니다.</summary>
        public static float CalculateMascotSize(float panelWidth) => Mathf.Clamp(panelWidth * 0.25f, 155f, 196f);

        /// <summary>하단 Safe Area 안의 Navigation이 마지막 Board 행과 겹치지 않도록 실제 높이를 예약합니다.</summary>
        public static float CalculateNavigationReserve(VisualElement navigation)
        {
            if (navigation == null || navigation.ClassListContains("hidden")) return 0f;
            var height = navigation.resolvedStyle.height;
            return float.IsNaN(height) || height <= 0f ? 80f : height + navigation.resolvedStyle.marginTop;
        }

        private void ApplyResponsiveMascotSize()
        {
            if (_mascotImage == null || _screen == null) return;
            var size = CalculateMascotSize(_screen.contentRect.width);
            _mascotImage.style.width = size;
            _mascotImage.style.height = size;
        }
        /// <summary>HUD 그림은 표시 전용 카탈로그에서 읽으며 재화 값이나 생성 결과에는 관여하지 않습니다.</summary>
        private void ApplyHudArt(VisualElement root)
        {
            if (_hudArt == null) return;
            var roomBackground = root.Q("room-background");
            if (_hudArt.roomBackground != null)
                roomBackground.style.backgroundImage = new StyleBackground(_hudArt.roomBackground);
            ApplyNavigationIcon(root, "nav-home-icon", _hudArt.navHome);
            ApplyNavigationIcon(root, "nav-collection-icon", _hudArt.navCollection);
            ApplyNavigationIcon(root, "nav-shop-icon", _hudArt.navShop);
            ApplyNavigationIcon(root, "nav-quest-icon", _hudArt.navQuest);
            if (_hudArt.toyGenerator != null) _generator.style.backgroundImage = new StyleBackground(_hudArt.toyGenerator);
            var energyIcon = root.Q("energy-icon");
            if (_hudArt.energy != null) energyIcon.style.backgroundImage = new StyleBackground(_hudArt.energy);
            var coinIcon = root.Q("coin-icon");
            if (_hudArt.coin != null) coinIcon.style.backgroundImage = new StyleBackground(_hudArt.coin);
            var gemIcon = root.Q("gem-icon");
            if (_hudArt.gem != null) gemIcon.style.backgroundImage = new StyleBackground(_hudArt.gem);
            ShowMascot(_hudArt.defaultMascot, "같은 장난감을 합쳐봐!");
        }

        /// <summary>Navigation 전체 Button은 Touch Area로 유지하고 하위 Icon에는 표시 Sprite만 연결합니다.</summary>
        private static void ApplyNavigationIcon(VisualElement root, string elementName, Sprite sprite)
        {
            var icon = root.Q(elementName);
            if (icon == null || sprite == null) return;
            icon.style.backgroundImage = new StyleBackground(sprite);
        }

        /// <summary>
        /// Presentation용 개별 고양이 Sprite를 표시합니다. Sprite가 없거나 화면 높이가 부족하면
        /// 보드를 축소하지 않고 마스코트 영역을 숨깁니다.
        /// </summary>
        public void ShowMascot(Sprite sprite, string message = null)
        {
            _mascotRequested = sprite != null;
            if (_mascotImage != null)
            {
                if (sprite == null) _mascotImage.style.backgroundImage = StyleKeyword.None;
                else _mascotImage.style.backgroundImage = new StyleBackground(sprite);
            }
            if (message != null) SetMascotMessage(message);
            UpdateMascotVisibility();
            if (_mascotRoot == null || !_mascotRequested || IsCompactMascotViewport(_viewportHeight)) return;

            _mascotRoot.AddToClassList("mascot-enter-start");
            _mascotRoot.schedule.Execute(() =>
            {
                _mascotRoot.RemoveFromClassList("mascot-enter-start");
                _mascotRoot.AddToClassList("mascot-enter-peak");
            }).StartingIn(20);
            _mascotRoot.schedule.Execute(() => _mascotRoot.RemoveFromClassList("mascot-enter-peak")).StartingIn(180);
        }

        /// <summary>마스코트 이미지를 제거하고 영역 전체를 숨겨 빈 UI가 사용자에게 보이지 않게 합니다.</summary>
        public void HideMascot()
        {
            _mascotRequested = false;
            if (_mascotImage != null) _mascotImage.style.backgroundImage = StyleKeyword.None;
            UpdateMascotVisibility();
        }

        /// <summary>별도 대화 시스템 없이 현재 말풍선 문구만 교체합니다.</summary>
        public void SetMascotMessage(string message)
        {
            if (_mascotMessage != null) _mascotMessage.text = message ?? string.Empty;
        }

        /// <summary>짧은 화면에서는 보드가 최우선이므로 마스코트를 표시하지 않습니다.</summary>
        public static bool IsCompactMascotViewport(float viewportHeight) => viewportHeight < 980f;

        private void UpdateMascotVisibility()
        {
            if (_mascotRoot == null) return;
            _mascotRoot.EnableInClassList("hidden", !_mascotRequested || IsCompactMascotViewport(_viewportHeight));
        }
        private void OnPointerDown(PointerDownEvent evt)
        {
            if (_inputBlocked) return;
            var element = FindSlot(evt.target as VisualElement);
            if (element == null || element.userData is not BoardSlotView slot) return;
            if (slot.IsEmpty) return; // 아이템 생성은 보드 빈 칸이 아니라 전용 생성기만 담당합니다.
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
            var validMerge = _dragging && hasTarget && BoardMergeRules.CanMerge(_dragSource, target);
            if (validMerge)
                MergeRequested?.Invoke(_dragSource.SlotIndex, target.SlotIndex);
            else if (_dragging)
                PlayInvalidDrop(_dragElement);
            CancelDrag(); evt.StopPropagation();
        }
        private void OnPointerCancel(PointerCancelEvent evt) { if (evt.pointerId == _pointerId) CancelDrag(); }
        private void BeginDrag()
        {
            _dragging = true; _dragElement.AddToClassList("board-slot-dragging");
            // 드래그 중에도 이름·레벨 텍스트 대신 실제 Sprite만 보여 상용 화면의 시각적 일관성을 유지합니다.
            _dragGhost = new VisualElement { pickingMode = PickingMode.Ignore };
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

        private static void PlayInvalidDrop(VisualElement source)
        {
            if (source == null) return;
            source.AddToClassList("board-slot-returning");
            source.schedule.Execute(() => source.RemoveFromClassList("board-slot-returning"))
                .StartingIn(InvalidDropFeedbackDurationMs);
        }

        /// <summary>서버가 Merge 성공 Board를 반환한 뒤 결과 Item에만 짧은 Pop과 별빛을 표시합니다.</summary>
        public void PlayMergeSuccess(int slotIndex) => PlaySuccessFeedback(slotIndex, "merge-success", 1.13f);

        /// <summary>서버가 Generate 성공 Board를 반환한 뒤 새 Item에만 짧은 Pop을 표시합니다.</summary>
        public void PlayGenerateSuccess(int slotIndex) => PlaySuccessFeedback(slotIndex, "generate-success", 1.09f);

        private void PlaySuccessFeedback(int slotIndex, string effectClass, float peakScale)
        {
            if (!_slotElements.TryGetValue(slotIndex, out var slot)) return;
            var art = slot.Q("item-art");
            if (art == null || art.ClassListContains("hidden")) return;

            var state = (BoardSlotView)slot.userData;
            var baseVisualScale = _itemArt?.FindVisualScale(state.ChainId, state.Level) ?? 1f;
            ApplyAnimationScale(art, baseVisualScale, 0.82f);
            slot.AddToClassList(effectClass);
            AddSparkle(slot, "sparkle-one"); AddSparkle(slot, "sparkle-two");
            art.schedule.Execute(() => ApplyAnimationScale(art, baseVisualScale, peakScale)).StartingIn(30);
            art.schedule.Execute(() => ApplyAnimationScale(art, baseVisualScale, 1f)).StartingIn(170);
            slot.schedule.Execute(() =>
            {
                slot.RemoveFromClassList(effectClass);
                slot.Q("sparkle-one")?.RemoveFromHierarchy(); slot.Q("sparkle-two")?.RemoveFromHierarchy();
            }).StartingIn(SuccessFeedbackDurationMs);
        }

        /// <summary>아트 고유 배율과 일시적 연출 배율을 곱해 Animation 종료 후에도 보정값을 보존합니다.</summary>
        public static float ComposeVisualScale(float baseVisualScale, float animationScale) =>
            Mathf.Max(0.01f, baseVisualScale) * Mathf.Max(0.01f, animationScale);

        private static void ApplyAnimationScale(VisualElement art, float baseVisualScale, float animationScale)
        {
            var composed = ComposeVisualScale(baseVisualScale, animationScale);
            art.style.scale = new Scale(new Vector3(composed, composed, 1f));
        }

        private static void AddSparkle(VisualElement slot, string className)
        {
            var sparkle = new Label("✦") { name = className, pickingMode = PickingMode.Ignore };
            sparkle.AddToClassList("merge-sparkle"); sparkle.AddToClassList(className); slot.Add(sparkle);
        }
        private static VisualElement FindSlot(VisualElement element)
        {
            while (element != null && element.userData is not BoardSlotView) element = element.parent;
            return element;
        }
        /// <summary>로컬에서 값을 확정하지 않고 요청 불가 이유만 사용자에게 안내합니다.</summary>
        public void SetStatus(string message)
        {
            if (_status == null) return;
            _status.text = message ?? string.Empty;
            _root?.Q("status-panel")?.EnableInClassList("hidden", string.IsNullOrWhiteSpace(message));
        }
    }
}
