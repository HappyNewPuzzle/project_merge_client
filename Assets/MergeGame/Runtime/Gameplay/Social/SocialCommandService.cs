using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.State;

namespace MergeGame.Client.Gameplay.Social
{
    public enum SocialOutcome { Succeeded, Replayed, Failed }
    public sealed class SocialCommandResult
    {
        public SocialOutcome Outcome { get; internal set; }
        public SocialState State { get; internal set; }
        public string FriendPlayerId { get; internal set; }
        public EconomySnapshot RecipientEconomy { get; internal set; }
        public ApiError Error { get; internal set; }
    }

    /// <summary>친구 관계와 선물 결과를 서버 응답으로만 갱신하는 소셜 명령 계층입니다.</summary>
    public sealed class SocialCommandService
    {
        private readonly IMergeGameApiClient _api; private readonly IGameStateStore _state;
        public SocialCommandService(IMergeGameApiClient api, IGameStateStore state) { _api = api; _state = state; }
        public IEnumerator Reload(Action<SocialCommandResult> completed)
        {
            ApiResult<SocialState> response = null; yield return _api.GetSocialProfile(value => response = value);
            if (response?.IsSuccess != true) { completed?.Invoke(Failed(response?.Error)); yield break; }
            _state.ApplySocial(response.Data);
            completed?.Invoke(new SocialCommandResult { Outcome = SocialOutcome.Succeeded, State = response.Data });
        }
        public IEnumerator AddFriend(string friendCode, Action<SocialCommandResult> completed)
        {
            ApiResult<AddFriendResponse> response = null;
            yield return _api.AddFriend(new AddFriendRequest { friendCode = friendCode?.Trim().ToUpperInvariant() ?? "" }, value => response = value);
            if (response?.IsSuccess != true) { completed?.Invoke(Failed(response?.Error)); yield break; }
            SocialCommandResult reload = null; yield return Reload(value => reload = value);
            if (reload?.Outcome != SocialOutcome.Succeeded) { completed?.Invoke(reload); yield break; }
            reload.Outcome = response.Data.alreadyFriends ? SocialOutcome.Replayed : SocialOutcome.Succeeded;
            reload.FriendPlayerId = response.Data.friendPlayerId; completed?.Invoke(reload);
        }
        public IEnumerator SendEnergyGift(string friendPlayerId, Action<SocialCommandResult> completed)
        {
            ApiResult<EnergyGiftResponse> response = null; yield return _api.SendFriendEnergyGift(friendPlayerId, value => response = value);
            if (response?.IsSuccess != true) { completed?.Invoke(Failed(response?.Error)); yield break; }
            // recipientEconomy는 친구 상태이므로 내 GameStateStore.Economy에 적용하면 안 됩니다.
            completed?.Invoke(new SocialCommandResult
            {
                Outcome = response.Data.replayed ? SocialOutcome.Replayed : SocialOutcome.Succeeded,
                RecipientEconomy = response.Data.recipientEconomy
            });
        }
        private static SocialCommandResult Failed(ApiError error) => new() { Outcome = SocialOutcome.Failed, Error = error };
    }
}

