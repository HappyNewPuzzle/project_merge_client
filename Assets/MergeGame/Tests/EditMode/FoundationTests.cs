using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.Configuration;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.Gameplay.Progression;
using MergeGame.Client.Presentation;

namespace MergeGame.Client.Tests.EditMode
{
    public sealed class FoundationTests
    {
        [Test] public void InMemoryStore_ReplacesRotatedSessionAtomically()
        {
            var store = new InMemoryTokenStore();
            store.SaveSession(new AuthSession("p", "a1", "", "r1", ""));
            store.SaveSession(new AuthSession("p", "a2", "", "r2", ""));
            Assert.That(store.LoadSession().AccessToken, Is.EqualTo("a2"));
            Assert.That(store.LoadSession().RefreshToken, Is.EqualTo("r2"));
        }
        [TestCase(401, "", ApiErrorKind.Unauthorized)]
        [TestCase(403, "account_suspended", ApiErrorKind.AccountSuspended)]
        [TestCase(409, "stale_revision", ApiErrorKind.RevisionConflict)]
        public void HttpErrors_AreClassified(long status, string code, ApiErrorKind expected)
        {
            var error = MergeGameApiClient.ClassifyError(UnityWebRequest.Result.ProtocolError, status, new ApiProblem { code = code }, "");
            Assert.That(error.Kind, Is.EqualTo(expected));
        }
        [Test] public void ConnectionError_IsNetworkError() =>
            Assert.That(MergeGameApiClient.ClassifyError(UnityWebRequest.Result.ConnectionError, 0, null, "offline").Kind, Is.EqualTo(ApiErrorKind.Network));
        [Test] public void EnvironmentCatalog_UsesHttpsAndContainsNoCredential()
        {
            var endpoint = ServerEndpointCatalog.For(ServerEnvironment.Production);
            Assert.That(endpoint.BaseUrl, Does.StartWith("https://"));
            Assert.That(endpoint.BaseUrl, Does.Not.Contain("@"));
        }
        [Test] public void SecureTokenStore_RoundTripsThroughPlatformBoundary()
        {
            var secrets = new FakeSecrets(); var store = new SecureTokenStore(secrets);
            store.SaveGuestCredential(new GuestCredential("player", "guest-secret"));
            store.SaveSession(new AuthSession("player", "access-secret", "a-exp", "refresh-secret", "r-exp"));
            Assert.That(store.LoadGuestCredential().PlayerId, Is.EqualTo("player"));
            Assert.That(store.LoadSession().RefreshToken, Is.EqualTo("refresh-secret"));
            store.ClearSession();
            Assert.That(store.LoadSession(), Is.Null);
        }
        private sealed class FakeSecrets : IPlatformSecretStore
        {
            private readonly Dictionary<string, string> _values = new();
            public string Get(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Set(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }
        [Test] public void BoardPresentation_CreatesServerSizedSlotsWithoutInventingItems()
        {
            var board = new BoardState
            {
                width = 2, height = 2,
                items = new[] { new BoardItemState { slotIndex = 2, itemId = "item", name = "Seed", level = 1 } }
            };
            var slots = BoardPresentationState.Create(board);
            Assert.That(slots, Has.Length.EqualTo(4));
            Assert.That(slots[0].IsEmpty, Is.True);
            Assert.That(slots[2].ItemId, Is.EqualTo("item"));
            Assert.That(slots[2].Level, Is.EqualTo(1));
        }
        [Test] public void BoardMergeRules_RequireSameChainLevelAndDifferentSlots()
        {
            var source = new BoardSlotView(0, new BoardItemState { itemId = "a", chainId = "workshop", level = 2 });
            var compatible = new BoardSlotView(1, new BoardItemState { itemId = "b", chainId = "workshop", level = 2 });
            var wrongLevel = new BoardSlotView(2, new BoardItemState { itemId = "c", chainId = "workshop", level = 3 });
            var wrongChain = new BoardSlotView(3, new BoardItemState { itemId = "d", chainId = "garden", level = 2 });
            Assert.That(BoardMergeRules.CanMerge(source, compatible), Is.True);
            Assert.That(BoardMergeRules.CanMerge(source, wrongLevel), Is.False);
            Assert.That(BoardMergeRules.CanMerge(source, wrongChain), Is.False);
            Assert.That(BoardMergeRules.CanMerge(source, source), Is.False);
        }
        [Test] public void BoardMergeRules_RejectServerMarkedMaxLevel()
        {
            var source = new BoardSlotView(0, new BoardItemState { itemId = "a", chainId = "toy", level = 8, isMaxLevel = true });
            var target = new BoardSlotView(1, new BoardItemState { itemId = "b", chainId = "toy", level = 8, isMaxLevel = true });
            Assert.That(BoardMergeRules.CanMerge(source, target), Is.False);
        }
        [Test] public void GeneratorPlacement_SelectsFirstEmptyAndRejectsFullBoard()
        {
            var empty = new BoardSlotView(3, null);
            var occupied = new BoardSlotView(1, new BoardItemState { itemId = "item", chainId = "workshop", level = 1 });
            Assert.That(BoardGeneratorPlacement.FindFirstEmpty(new[] { occupied, empty }), Is.EqualTo(3));
            Assert.That(BoardGeneratorPlacement.FindFirstEmpty(new[] { occupied }), Is.EqualTo(-1));
        }
        [Test] public void SafeAreaInsets_ConvertScreenPixelsToPanelUnits()
        {
            var insets = SafeAreaController.CalculateInsets(new Rect(0, 80, 1080, 2240), 1080, 2400, 720, 1280);
            Assert.That(insets.Left, Is.EqualTo(0));
            Assert.That(insets.Top, Is.EqualTo(42.666f).Within(0.01f));
            Assert.That(insets.Right, Is.EqualTo(0));
            Assert.That(insets.Bottom, Is.EqualTo(42.666f).Within(0.01f));
        }
        [TestCase(1280f, 112f)]
        [TestCase(900f, 112f)]
        [TestCase(720f, 92.857f)]
        [TestCase(560f, 62f)]
        public void BoardSlotHeight_RespondsToAvailableVerticalSpace(float viewportHeight, float expected)
        {
            Assert.That(GameHudPresenter.CalculateBoardSlotHeight(viewportHeight), Is.EqualTo(expected).Within(0.01f));
        }
        [TestCase(720f, 200f, 14f, 5f, 112f)]
        [TestCase(640f, 260f, 12f, 4f, 87f)]
        [TestCase(480f, 280f, 12f, 4f, 48f)]
        public void FittedBoardHeight_UsesActualRemainingViewport(
            float viewport, float boardTop, float frameExtras, float margins, float expected)
        {
            Assert.That(GameHudPresenter.CalculateFittedBoardSlotHeight(viewport, boardTop, frameExtras, margins),
                Is.EqualTo(expected).Within(0.01f));
        }
        [Test] public void PortraitBoard_UsesSquareCellsWithoutConsumingMascotSpace()
        {
            Assert.That(GameHudPresenter.CalculatePortraitBoardSlotHeight(1280f, 330f, 14f, 5f, 612f, 180f),
                Is.EqualTo(148f).Within(0.01f));
            Assert.That(GameHudPresenter.CalculatePortraitBoardSlotHeight(900f, 250f, 14f, 5f, 612f, 0f),
                Is.EqualTo(148f).Within(0.01f));
            Assert.That(GameHudPresenter.CalculatePortraitBoardSlotHeight(720f, 300f, 14f, 5f, 612f, 0f),
                Is.EqualTo(95.5f).Within(0.01f));
        }
        [TestCase(620f, 155f)]
        [TestCase(720f, 180f)]
        [TestCase(900f, 196f)]
        public void MascotSize_TracksPanelWidthWithinReadableLimits(float width, float expected)
        {
            Assert.That(GameHudPresenter.CalculateMascotSize(width), Is.EqualTo(expected).Within(0.01f));
        }
        [Test] public void NavigationReserve_IsSafeBeforeNavigationExists()
        {
            Assert.That(GameHudPresenter.CalculateNavigationReserve(null), Is.Zero);
        }
        [Test] public void ItemVisualSizing_NormalizesTransparentPaddingWithoutChangingSprites()
        {
            var catalog = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            Assert.That(catalog.toyVisualScales, Has.Length.EqualTo(8));
            Assert.That(catalog.foodVisualScales, Has.Length.EqualTo(8));
            Assert.That(catalog.restVisualScales, Has.Length.EqualTo(8));
            Assert.That(catalog.FindVisualScale("toy", 1), Is.EqualTo(1.69f));
            Assert.That(catalog.FindVisualScale("toy", 4), Is.EqualTo(1.25f));
            Assert.That(catalog.FindVisualScale("toy", 8), Is.EqualTo(0.90f));
            Assert.That(catalog.FindVisualScale("food", 1), Is.GreaterThan(catalog.FindVisualScale("food", 8)));
            Assert.That(catalog.FindVisualScale("rest", 1), Is.InRange(0.9f, 1f));
            Assert.That(catalog.FindVisualScale("unknown", 1), Is.EqualTo(1f));
        }
        [Test] public void ItemAnimation_ComposesWithAndRestoresBaseVisualScale()
        {
            Assert.That(GameHudPresenter.ComposeVisualScale(1.25f, 0.82f), Is.EqualTo(1.025f).Within(0.001f));
            Assert.That(GameHudPresenter.ComposeVisualScale(1.25f, 1.13f), Is.EqualTo(1.4125f).Within(0.001f));
            Assert.That(GameHudPresenter.ComposeVisualScale(1.25f, 1f), Is.EqualTo(1.25f).Within(0.001f));
        }
        [Test] public void Mascot_HidesBeforeBoardOnCompactViewport()
        {
            Assert.That(GameHudPresenter.IsCompactMascotViewport(0f), Is.True);
            Assert.That(GameHudPresenter.IsCompactMascotViewport(759f), Is.True);
            Assert.That(GameHudPresenter.IsCompactMascotViewport(979f), Is.True);
            Assert.That(GameHudPresenter.IsCompactMascotViewport(980f), Is.False);
        }
        [Test] public void BoardFeedbackDurations_RemainShortAndDoNotBecomeInputLocks()
        {
            Assert.That(GameHudPresenter.SuccessFeedbackDurationMs, Is.InRange(200, 400));
            Assert.That(GameHudPresenter.InvalidDropFeedbackDurationMs, Is.LessThanOrEqualTo(200));
        }
        [Test] public void WorkshopArtCatalog_ContainsNineOrderedSprites()
        {
            var catalog = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.levelSprites, Has.Length.EqualTo(9));
            Assert.That(catalog.Find("workshop", 1).name, Is.EqualTo("01_seed_packet"));
            Assert.That(catalog.Find("workshop", 9).name, Is.EqualTo("09_lantern"));
            Assert.That(catalog.Find("workshop", 1).rect.width, Is.EqualTo(418));
            Assert.That(catalog.Find("workshop", 1).rect.height, Is.EqualTo(418));
            Assert.That(catalog.Find("unknown", 1), Is.Null);
        }
        [Test] public void ItemArtCatalog_ContainsToyLevelsOneThroughEight()
        {
            var catalog = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            Assert.That(catalog.toySprites, Has.Length.EqualTo(8));
            Assert.That(catalog.Find("toy", 1).name, Is.EqualTo("Toy_Lv01"));
            Assert.That(catalog.Find("toy", 8).name, Is.EqualTo("Toy_Lv08"));
            Assert.That(catalog.Find("toy", 9), Is.Null);
        }
        [TestCase("toy", "Toy")]
        [TestCase("food", "Food")]
        [TestCase("rest", "Rest")]
        public void ItemArtCatalog_ContainsEightOrderedSpritesForPreparedArtLines(string chainId, string prefix)
        {
            var catalog = Resources.Load<WorkshopItemArtCatalog>("WorkshopItemArtCatalog");
            Assert.That(catalog.Find(chainId, 1).name, Is.EqualTo($"{prefix}_Lv01"));
            Assert.That(catalog.Find(chainId, 8).name, Is.EqualTo($"{prefix}_Lv08"));
            Assert.That(catalog.Find(chainId, 0), Is.Null);
            Assert.That(catalog.Find(chainId, 9), Is.Null);
            Assert.That(catalog.Find("unknown", 1), Is.Null);
        }
        [Test] public void HudArtCatalog_ContainsGeneratorAndCurrencySprites()
        {
            var catalog = Resources.Load<WorkshopHudArtCatalog>("WorkshopHudArtCatalog");
            Assert.That(catalog, Is.Not.Null);
            Assert.That(catalog.toyGenerator.name, Is.EqualTo("Generator_Toy"));
            Assert.That(catalog.foodGenerator.name, Is.EqualTo("Generator_Food"));
            Assert.That(catalog.restGenerator.name, Is.EqualTo("Generator_Rest"));
            Assert.That(catalog.coin.name, Is.EqualTo("Currency_Coin"));
            Assert.That(catalog.energy.name, Is.EqualTo("Currency_Energy"));
            Assert.That(catalog.gem.name, Is.EqualTo("Currency_Gem"));
            Assert.That(catalog.defaultMascot.name, Is.EqualTo("Cat_Mascot_Default"));
            Assert.That(catalog.roomBackground.name, Is.EqualTo("CatMerge_Room_Background"));
            Assert.That(catalog.navHome.name, Is.EqualTo("UI_Nav_Home"));
            Assert.That(catalog.navCollection.name, Is.EqualTo("UI_Nav_Collection"));
            Assert.That(catalog.navShop.name, Is.EqualTo("UI_Nav_Shop"));
            Assert.That(catalog.navQuest.name, Is.EqualTo("UI_Nav_Quest"));
        }
        [Test] public void QuestClaimIntent_KeepsKeyForTheSameUserIntent()
        {
            var intent = QuestClaimIntent.Create("first-merge");
            var sameReference = intent;
            Assert.That(intent.IdempotencyKey, Has.Length.EqualTo(32));
            Assert.That(sameReference.IdempotencyKey, Is.EqualTo(intent.IdempotencyKey));
            Assert.That(QuestClaimIntent.Create("first-merge").IdempotencyKey, Is.Not.EqualTo(intent.IdempotencyKey));
        }
        [TestCase(ApiErrorKind.Network, GameUiPhase.NetworkUnavailable)]
        [TestCase(ApiErrorKind.Unauthorized, GameUiPhase.AuthenticationRequired)]
        [TestCase(ApiErrorKind.AccountSuspended, GameUiPhase.AccountSuspended)]
        [TestCase(ApiErrorKind.RevisionConflict, GameUiPhase.ConflictResynchronized)]
        public void UiModel_MapsRecoveryStates(ApiErrorKind kind, GameUiPhase phase)
        {
            var model = new GameUiModel(); model.ApplyError(ApiResult<object>.Failure(kind).Error);
            Assert.That(model.Phase, Is.EqualTo(phase));
        }
        [Test] public void RetryPolicy_NeverRetriesUnknownMutationButAllowsStableIdempotencyKey()
        {
            var network = ApiResult<object>.Failure(ApiErrorKind.Network).Error;
            Assert.That(NetworkRetryPolicy.CanRetry(network, false, false, 0), Is.False);
            Assert.That(NetworkRetryPolicy.CanRetry(network, false, true, 0), Is.True);
            Assert.That(NetworkRetryPolicy.CanRetry(network, true, false, 2), Is.False);
        }
        [Test] public void Diagnostics_BoundsMemoryAndStoresOnlySafeMetadata()
        {
            var diagnostics = new ClientDiagnostics(2);
            diagnostics.Record(new ApiObservation(200, ApiErrorKind.None, 10, "a"));
            diagnostics.Record(new ApiObservation(401, ApiErrorKind.Unauthorized, 20, "b"));
            diagnostics.Record(new ApiObservation(409, ApiErrorKind.RevisionConflict, 30, "c"));
            Assert.That(diagnostics.Items, Has.Count.EqualTo(2));
        }

        [Test] public void SupportSnapshot_ExcludesSensitiveServerMessage()
        {
            var error = MergeGameApiClient.ClassifyError(UnityWebRequest.Result.ProtocolError, 403,
                new ApiProblem { code = "account_suspended", message = "private moderation note", traceId = "trace-safe" }, null);
            var text = SupportDiagnosticSnapshot.From(error).ToSupportText();
            Assert.That(text, Does.Contain("AccountSuspended"));
            Assert.That(text, Does.Contain("trace-safe"));
            Assert.That(text, Does.Not.Contain("private moderation note"));
            Assert.That(text, Does.Not.Contain("token"));
        }

    }
}
