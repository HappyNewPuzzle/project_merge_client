using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MergeGame.Client.Api
{
    /// <summary>다음 Mock 요청 하나에 주입할 복구 시나리오입니다. 기본값은 정상 응답입니다.</summary>
    public enum MockApiScenario { Success, NetworkError, Unauthorized, AccountSuspended, RevisionConflict }

    /// <summary>
    /// Unity 클라이언트가 값을 임의 확정하지 않도록 실제 API와 같은 경계 뒤에서 동작하는 개발용 가상 서버입니다.
    /// 모든 보드·경제·퀘스트 revision 변경은 이 객체 내부에서 처리된 뒤 응답 스냅샷으로만 공개됩니다.
    /// </summary>
    public sealed class MockMergeGameApiClient : IMergeGameApiClient
    {
        private readonly MockServerState _state;
        public string AccessToken { get; set; } = "";
        public MockApiScenario NextScenario { get; set; }
        public int LatencyFrames { get; set; }

        public MockMergeGameApiClient(MockServerState state = null) => _state = state ?? new MockServerState();

        public IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> completed) => Respond(completed, () => new CreateGuestPlayerResponse
        { playerId = _state.PlayerId, displayName = "Offline Guest", guestToken = "mock-guest-credential", createdAtUtc = Now() });
        public IEnumerator LoginGuest(GuestLoginRequest body, Action<ApiResult<GuestLoginResponse>> completed) => Respond(completed, () =>
        {
            if (body?.playerId != _state.PlayerId || string.IsNullOrWhiteSpace(body.guestToken)) throw new MockUnauthorizedException();
            AccessToken = "mock-access-token";
            return Session();
        });
        public IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> completed) => Respond(completed, () =>
        {
            if (body?.refreshToken != "mock-refresh-token") throw new MockUnauthorizedException();
            AccessToken = "mock-access-token-refreshed";
            return Session();
        });
        public IEnumerator Logout(RefreshTokenRequest body, Action<ApiResult<EmptyResponse>> completed) => Respond(completed, () => { AccessToken = ""; return new EmptyResponse(); });
        public IEnumerator GetCurrentPlayer(Action<ApiResult<CurrentPlayerResponse>> completed) => Respond(completed, () => new CurrentPlayerResponse { playerId = _state.PlayerId, displayName = "Offline Guest", createdAtUtc = Now() });
        public IEnumerator InitializeBoard(Action<ApiResult<BoardState>> completed) => Respond(completed, () => Snapshot(_state.Board));
        public IEnumerator GetBoard(Action<ApiResult<BoardState>> completed) => Respond(completed, () => Snapshot(_state.Board));
        public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> completed) => Respond(completed, () => Snapshot(_state.Economy));
        public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> completed) => Respond(completed, () => Snapshot(_state.Economy));
        public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> completed) => Respond(completed, () => Snapshot(_state.Quest));
        public IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> completed) => Respond(completed, () => Snapshot(_state.Quest));
        public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> completed) => Respond(completed, () => new SocialProfileSnapshot { friendCode = _state.FriendCode });
        public IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> completed) => Respond(completed, () => new SocialState { friendCode = _state.FriendCode, friends = _state.Friends.ToArray() });

        public IEnumerator GenerateItem(GenerateItemRequest body, Action<ApiResult<GenerateItemResponse>> completed) => Respond(completed, () =>
        {
            if (body.expectedBoardRevision != _state.Board.revision || body.expectedEconomyRevision != _state.Economy.revision) throw new MockConflictException();
            if (_state.Economy.energy <= 0) throw new MockHttpException("insufficient_energy");
            if (body.targetSlot < 0 || body.targetSlot >= _state.Board.width * _state.Board.height || Find(body.targetSlot) != null) throw new MockHttpException("invalid_target");
            var items = new List<BoardItemState>(_state.Board.items) { new() { itemId = Guid.NewGuid().ToString("N"), slotIndex = body.targetSlot, chainId = "toy", level = 1, name = "Toy Lv.01" } };
            _state.Board.items = items.ToArray(); _state.Board.revision++; _state.Economy.energy--; _state.Economy.revision++;
            return new GenerateItemResponse { board = Snapshot(_state.Board), economy = Snapshot(_state.Economy) };
        });
        public IEnumerator MergeItems(MergeBoardItemsRequest body, Action<ApiResult<BoardState>> completed) => Respond(completed, () =>
        {
            if (body.expectedRevision != _state.Board.revision) throw new MockConflictException();
            var source = Find(body.sourceSlot); var target = Find(body.targetSlot);
            if (source == null || target == null || source.level != target.level || source.chainId != target.chainId || source.isMaxLevel) throw new MockHttpException("invalid_merge");
            var items = new List<BoardItemState>(_state.Board.items); items.Remove(source); target.level++;
            target.name = target.chainId == "toy" ? $"Toy Lv.{target.level:00}" : "Workshop Item Lv." + target.level;
            target.isMaxLevel = target.chainId == "toy" && target.level >= 8;
            _state.Board.items = items.ToArray(); _state.Board.revision++; _state.Quest.currentCount++; _state.Quest.revision++;
            _state.Quest.isCompleted = _state.Quest.currentCount >= _state.Quest.targetCount; return Snapshot(_state.Board);
        });
        public IEnumerator ClaimDailyReward(RevisionRequest body, Action<ApiResult<EconomySnapshot>> completed) => Respond(completed, () =>
        {
            if (body.expectedRevision != _state.Economy.revision) throw new MockConflictException();
            _state.Economy.coins += 50; _state.Economy.dailyRewardClaimedToday = true; _state.Economy.revision++; return Snapshot(_state.Economy);
        });
        public IEnumerator ClaimQuestReward(string questId, ClaimQuestRewardRequest body, Action<ApiResult<QuestRewardResponse>> completed) => Respond(completed, () =>
        {
            if (body.expectedQuestRevision != _state.Quest.revision || body.expectedEconomyRevision != _state.Economy.revision) throw new MockConflictException();
            if (!_state.Quest.isCompleted || _state.Quest.isClaimed) throw new MockHttpException("quest_not_claimable");
            _state.Quest.isClaimed = true; _state.Quest.revision++; _state.Economy.coins += _state.Quest.rewardCoins; _state.Economy.revision++;
            return new QuestRewardResponse { quest = Snapshot(_state.Quest), economy = Snapshot(_state.Economy) };
        });
        public IEnumerator AddFriend(AddFriendRequest body, Action<ApiResult<AddFriendResponse>> completed) => Respond(completed, () =>
        {
            var existing = _state.Friends.Find(value => value.playerId == "mock-friend");
            if (existing == null) _state.Friends.Add(new FriendSnapshot { playerId = "mock-friend", displayName = "Mock Friend", friendsSinceUtc = Now() });
            return new AddFriendResponse { alreadyFriends = existing != null, friendPlayerId = "mock-friend" };
        });
        public IEnumerator SendFriendEnergyGift(string playerId, Action<ApiResult<EnergyGiftResponse>> completed) => Respond(completed, () => new EnergyGiftResponse
        { recipientEconomy = new EconomySnapshot { playerId = playerId, energy = 6, maxEnergy = 10, revision = 2 } });

        private IEnumerator Respond<T>(Action<ApiResult<T>> completed, Func<T> create)
        {
            for (var frame = 0; frame < LatencyFrames; frame++) yield return null;
            var scenario = NextScenario; NextScenario = MockApiScenario.Success;
            if (scenario != MockApiScenario.Success) { completed?.Invoke(Failure<T>(scenario)); yield break; }
            try { completed?.Invoke(ApiResult<T>.Success(create())); }
            catch (MockUnauthorizedException) { completed?.Invoke(ApiResult<T>.Failure(ApiErrorKind.Unauthorized, 401, "unauthorized")); }
            catch (MockConflictException) { completed?.Invoke(ApiResult<T>.Failure(ApiErrorKind.RevisionConflict, 409, "stale_revision")); }
            catch (MockHttpException error) { completed?.Invoke(ApiResult<T>.Failure(ApiErrorKind.Http, 400, error.Code)); }
        }
        private static ApiResult<T> Failure<T>(MockApiScenario scenario) => scenario switch
        {
            MockApiScenario.NetworkError => ApiResult<T>.Failure(ApiErrorKind.Network, 0, "mock_network"),
            MockApiScenario.Unauthorized => ApiResult<T>.Failure(ApiErrorKind.Unauthorized, 401, "unauthorized"),
            MockApiScenario.AccountSuspended => ApiResult<T>.Failure(ApiErrorKind.AccountSuspended, 403, "account_suspended"),
            MockApiScenario.RevisionConflict => ApiResult<T>.Failure(ApiErrorKind.RevisionConflict, 409, "stale_revision"),
            _ => ApiResult<T>.Failure(ApiErrorKind.Http, 500, "mock_failure")
        };
        private BoardItemState Find(int slot) => Array.Find(_state.Board.items, value => value.slotIndex == slot);
        // JSON 직렬화 복사는 실제 HTTP 경계처럼 호출자가 받은 객체를 바꿔도 가상 서버 원본이 변하지 않게 합니다.
        private static T Snapshot<T>(T value) => JsonUtility.FromJson<T>(JsonUtility.ToJson(value));
        private GuestLoginResponse Session() => new() { playerId = _state.PlayerId, accessToken = AccessToken, refreshToken = "mock-refresh-token", expiresAtUtc = Now(), refreshTokenExpiresAtUtc = Now() };
        private static string Now() => DateTime.UtcNow.ToString("O");
        private sealed class MockUnauthorizedException : Exception { }
        private sealed class MockConflictException : Exception { }
        private sealed class MockHttpException : Exception { public string Code { get; } public MockHttpException(string code) => Code = code; }
    }

    /// <summary>Mock API만 변경할 수 있는 가상 서버 권위 상태입니다.</summary>
    public sealed class MockServerState
    {
        public string PlayerId { get; } = "mock-player";
        public string FriendCode { get; } = "MOCK01";
        public BoardState Board { get; } = new() { playerId = "mock-player", width = 4, height = 4, revision = 1 };
        public EconomySnapshot Economy { get; } = new() { playerId = "mock-player", energy = 10, maxEnergy = 10, coins = 100, revision = 1 };
        public QuestSnapshot Quest { get; } = new() { questId = "merge-two", targetCount = 1, rewardCoins = 25, revision = 1 };
        public List<FriendSnapshot> Friends { get; } = new();
    }
}
