using System;
using System.Collections;

namespace MergeGame.Client.Api
{
    /// <summary>Bootstrap과 테스트가 실제 전송 구현에 결합되지 않도록 공개 API 경계를 정의합니다.</summary>
    public interface IMergeGameApiClient
    {
        string AccessToken { get; set; }
        IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> completed);
        IEnumerator LoginGuest(GuestLoginRequest body, Action<ApiResult<GuestLoginResponse>> completed);
        IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> completed);
        IEnumerator Logout(RefreshTokenRequest body, Action<ApiResult<EmptyResponse>> completed);
        IEnumerator GetCurrentPlayer(Action<ApiResult<CurrentPlayerResponse>> completed);
        IEnumerator InitializeBoard(Action<ApiResult<BoardState>> completed);
        IEnumerator GetBoard(Action<ApiResult<BoardState>> completed);
        IEnumerator MergeItems(MergeBoardItemsRequest body, Action<ApiResult<BoardState>> completed);
        IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> completed);
        IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> completed);
        IEnumerator GenerateItem(GenerateItemRequest body, Action<ApiResult<GenerateItemResponse>> completed);
        IEnumerator ClaimDailyReward(RevisionRequest body, Action<ApiResult<EconomySnapshot>> completed);
        IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> completed);
        IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> completed);
        IEnumerator ClaimQuestReward(string questId, ClaimQuestRewardRequest body, Action<ApiResult<QuestRewardResponse>> completed);
        IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> completed);
        IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> completed);
        IEnumerator AddFriend(AddFriendRequest body, Action<ApiResult<AddFriendResponse>> completed);
        IEnumerator SendFriendEnergyGift(string playerId, Action<ApiResult<EnergyGiftResponse>> completed);
    }
}
