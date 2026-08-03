using System;

namespace MergeGame.Client.Api
{
    // JsonUtility가 public 필드를 직렬화하므로 서버의 camelCase JSON 이름과 정확히 맞춥니다.
    // 날짜는 플랫폼별 DateTime 변환 차이를 피하기 위해 ISO 8601 문자열로 유지합니다.
    [Serializable] public sealed class CreateGuestPlayerResponse { public string playerId = ""; public string displayName = ""; public string guestToken = ""; public string createdAtUtc = ""; }
    [Serializable] public sealed class GuestLoginRequest { public string playerId = ""; public string guestToken = ""; }
    [Serializable] public sealed class GuestLoginResponse { public string playerId = ""; public string accessToken = ""; public string tokenType = "Bearer"; public string expiresAtUtc = ""; public string refreshToken = ""; public string refreshTokenExpiresAtUtc = ""; }
    [Serializable] public sealed class RefreshTokenRequest { public string refreshToken = ""; }
    [Serializable] public sealed class CurrentPlayerResponse { public string playerId = ""; public string displayName = ""; public string createdAtUtc = ""; }
    [Serializable] public sealed class BoardItemState { public string itemId = ""; public int slotIndex; public string chainId = ""; public int level; public string name = ""; public bool isMaxLevel; }
    [Serializable] public sealed class BoardState { public string playerId = ""; public int width; public int height; public long revision; public BoardItemState[] items = Array.Empty<BoardItemState>(); }
    [Serializable] public sealed class MergeBoardItemsRequest { public int sourceSlot; public int targetSlot; public long expectedRevision; }
    [Serializable] public sealed class EconomySnapshot { public string playerId = ""; public int energy; public int maxEnergy; public long coins; public long revision; public string nextEnergyAtUtc = ""; public bool dailyRewardClaimedToday; }
    [Serializable] public sealed class GenerateItemRequest { public int targetSlot; public long expectedBoardRevision; public long expectedEconomyRevision; }
    [Serializable] public sealed class GenerateItemResponse { public BoardState board = new(); public EconomySnapshot economy = new(); }
    [Serializable] public sealed class RevisionRequest { public long expectedRevision; }
    [Serializable] public sealed class QuestSnapshot { public string questId = ""; public int currentCount; public int targetCount; public long rewardCoins; public long revision; public bool isCompleted; public bool isClaimed; }
    [Serializable] public sealed class ClaimQuestRewardRequest { public string idempotencyKey = ""; public long expectedQuestRevision; public long expectedEconomyRevision; }
    [Serializable] public sealed class QuestRewardResponse { public bool replayed; public QuestSnapshot quest = new(); public EconomySnapshot economy = new(); public string error = ""; }
    [Serializable] public sealed class SocialProfileSnapshot { public string friendCode = ""; }
    [Serializable] public sealed class FriendSnapshot { public string playerId = ""; public string displayName = ""; public string friendsSinceUtc = ""; public bool energyGiftSentToday; }
    [Serializable] public sealed class SocialState { public string friendCode = ""; public FriendSnapshot[] friends = Array.Empty<FriendSnapshot>(); }
    [Serializable] public sealed class AddFriendRequest { public string friendCode = ""; }
    [Serializable] public sealed class AddFriendResponse { public bool alreadyFriends; public string friendPlayerId = ""; }
    [Serializable] public sealed class EnergyGiftResponse { public bool replayed; public EconomySnapshot recipientEconomy = new(); }
    [Serializable] public sealed class EmptyResponse { }

    [Serializable]
    public sealed class ApiProblem
    {
        public string title = ""; public int status; public string detail = ""; public string instance = "";
        public string code = ""; public string message = ""; public string traceId = "";
    }
}

