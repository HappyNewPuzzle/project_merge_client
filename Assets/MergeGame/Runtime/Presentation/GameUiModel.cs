using System;
using MergeGame.Client.Api;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.State;

namespace MergeGame.Client.Presentation
{
    public enum GameUiPhase { Bootstrapping, Ready, NetworkUnavailable, AuthenticationRequired, AccountSuspended, ConflictResynchronized, Error }
    public sealed class GameUiModel
    {
        public GameUiPhase Phase { get; private set; } = GameUiPhase.Bootstrapping;
        public string Message { get; private set; } = "서버 상태를 불러오는 중입니다.";
        public BoardSlotView[] Slots { get; private set; } = Array.Empty<BoardSlotView>();
        public int Energy { get; private set; } public long Coins { get; private set; }
        public string QuestText { get; private set; } = ""; public string FriendCode { get; private set; } = "";

        public void Apply(IGameStateStore state)
        {
            Slots = BoardPresentationState.Create(state.Board);
            Energy = state.Economy?.energy ?? 0; Coins = state.Economy?.coins ?? 0;
            QuestText = state.Quest == null ? "" : $"{state.Quest.currentCount}/{state.Quest.targetCount}";
            FriendCode = state.Social?.friendCode ?? state.SocialProfile?.friendCode ?? "";
            Phase = GameUiPhase.Ready; Message = "준비 완료";
        }
        public void ApplyError(ApiError error)
        {
            Phase = error?.Kind switch
            {
                ApiErrorKind.Network => GameUiPhase.NetworkUnavailable,
                ApiErrorKind.Unauthorized => GameUiPhase.AuthenticationRequired,
                ApiErrorKind.AccountSuspended => GameUiPhase.AccountSuspended,
                ApiErrorKind.RevisionConflict => GameUiPhase.ConflictResynchronized,
                _ => GameUiPhase.Error
            };
            Message = Phase switch
            {
                GameUiPhase.NetworkUnavailable => KoreanStrings.NetworkUnavailable,
                GameUiPhase.AuthenticationRequired => KoreanStrings.AuthenticationRequired,
                GameUiPhase.AccountSuspended => KoreanStrings.AccountSuspended,
                GameUiPhase.ConflictResynchronized => KoreanStrings.Conflict,
                _ => string.IsNullOrWhiteSpace(error?.TraceId) ? "요청을 처리하지 못했습니다." : "요청을 처리하지 못했습니다. 문의 코드: " + error.TraceId
            };
        }
    }
}
