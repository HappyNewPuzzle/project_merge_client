using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.State;

namespace MergeGame.Client.Gameplay.Board
{
    public enum BoardCommandOutcome { Succeeded, ConflictResynchronized, Failed }
    public sealed class BoardCommandResult
    {
        public BoardCommandOutcome Outcome { get; internal set; }
        public BoardState Board { get; internal set; }
        public EconomySnapshot Economy { get; internal set; }
        public ApiError Error { get; internal set; }
    }

    /// <summary>
    /// 보드 변경 요청에 마지막 서버 revision을 넣고 성공 응답만 상태에 적용합니다.
    /// 클라이언트는 머지 결과 레벨, 에너지 소비량 또는 새 revision을 미리 확정하지 않습니다.
    /// </summary>
    public sealed class BoardCommandService
    {
        private readonly IMergeGameApiClient _api;
        private readonly IGameStateStore _state;
        public BoardCommandService(IMergeGameApiClient api, IGameStateStore state)
        { _api = api ?? throw new ArgumentNullException(nameof(api)); _state = state ?? throw new ArgumentNullException(nameof(state)); }

        public IEnumerator Reload(Action<BoardCommandResult> completed)
        {
            ApiResult<BoardState> board = null; ApiResult<EconomySnapshot> economy = null;
            yield return _api.GetBoard(value => board = value);
            if (board?.IsSuccess != true) { completed?.Invoke(Failed(board?.Error)); yield break; }
            yield return _api.GetEconomy(value => economy = value);
            if (economy?.IsSuccess != true) { completed?.Invoke(Failed(economy?.Error)); yield break; }
            _state.ApplyBoard(board.Data); _state.ApplyEconomy(economy.Data);
            completed?.Invoke(new BoardCommandResult { Outcome = BoardCommandOutcome.Succeeded, Board = board.Data, Economy = economy.Data });
        }

        public IEnumerator Merge(int sourceSlot, int targetSlot, Action<BoardCommandResult> completed)
        {
            if (_state.Board == null) { completed?.Invoke(Failed(ClientStateMissing("board"))); yield break; }
            ApiResult<BoardState> response = null;
            yield return _api.MergeItems(new MergeBoardItemsRequest
            {
                sourceSlot = sourceSlot, targetSlot = targetSlot, expectedRevision = _state.Board.revision
            }, value => response = value);
            if (response?.IsSuccess == true)
            {
                _state.ApplyBoard(response.Data);
                completed?.Invoke(new BoardCommandResult { Outcome = BoardCommandOutcome.Succeeded, Board = response.Data });
                yield break;
            }
            if (response?.Error?.Kind == ApiErrorKind.RevisionConflict)
            {
                yield return Resynchronize(response.Error, completed);
                yield break;
            }
            completed?.Invoke(Failed(response?.Error));
        }

        public IEnumerator Generate(int targetSlot, Action<BoardCommandResult> completed)
        {
            if (_state.Board == null || _state.Economy == null)
            { completed?.Invoke(Failed(ClientStateMissing("board_and_economy"))); yield break; }
            ApiResult<GenerateItemResponse> response = null;
            yield return _api.GenerateItem(new GenerateItemRequest
            {
                targetSlot = targetSlot,
                expectedBoardRevision = _state.Board.revision,
                expectedEconomyRevision = _state.Economy.revision
            }, value => response = value);
            if (response?.IsSuccess == true)
            {
                _state.ApplyBoard(response.Data.board); _state.ApplyEconomy(response.Data.economy);
                completed?.Invoke(new BoardCommandResult { Outcome = BoardCommandOutcome.Succeeded, Board = response.Data.board, Economy = response.Data.economy });
                yield break;
            }
            if (response?.Error?.Kind == ApiErrorKind.RevisionConflict)
            {
                yield return Resynchronize(response.Error, completed);
                yield break;
            }
            completed?.Invoke(Failed(response?.Error));
        }

        private IEnumerator Resynchronize(ApiError conflict, Action<BoardCommandResult> completed)
        {
            BoardCommandResult reload = null;
            yield return Reload(value => reload = value);
            if (reload?.Outcome != BoardCommandOutcome.Succeeded) { completed?.Invoke(reload); yield break; }
            reload.Outcome = BoardCommandOutcome.ConflictResynchronized;
            reload.Error = conflict;
            completed?.Invoke(reload); // 원래 변경은 재호출하지 않고 UI가 최신 상태에서 다시 판단합니다.
        }

        private static BoardCommandResult Failed(ApiError error) => new() { Outcome = BoardCommandOutcome.Failed, Error = error ?? ClientStateMissing("unknown") };
        private static ApiError ClientStateMissing(string target) => new() { Kind = ApiErrorKind.Http, Code = "client_state_missing", Message = target };
    }
}

