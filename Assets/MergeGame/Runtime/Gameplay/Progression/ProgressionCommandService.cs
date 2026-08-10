using System;
using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.State;

namespace MergeGame.Client.Gameplay.Progression
{
    public enum ProgressionOutcome { Succeeded, Replayed, ConflictResynchronized, Failed }
    public sealed class ProgressionResult
    {
        public ProgressionOutcome Outcome { get; internal set; }
        public EconomySnapshot Economy { get; internal set; }
        public QuestSnapshot Quest { get; internal set; }
        public ApiError Error { get; internal set; }
    }

    /// <summary>한 번의 보상 사용자 의도와 서버 멱등성 키를 같은 수명으로 묶습니다.</summary>
    public sealed class QuestClaimIntent
    {
        public string QuestId { get; }
        public string IdempotencyKey { get; }
        private QuestClaimIntent(string questId, string idempotencyKey) { QuestId = questId; IdempotencyKey = idempotencyKey; }
        public static QuestClaimIntent Create(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId)) throw new ArgumentException("questId가 필요합니다.", nameof(questId));
            return new QuestClaimIntent(questId, Guid.NewGuid().ToString("N"));
        }
    }

    /// <summary>경제·퀘스트 revision과 멱등성 키를 보존하며 서버 응답만 로컬 상태에 반영합니다.</summary>
    public sealed class ProgressionCommandService
    {
        private readonly IMergeGameApiClient _api; private readonly IGameStateStore _state;
        public ProgressionCommandService(IMergeGameApiClient api, IGameStateStore state)
        { _api = api ?? throw new ArgumentNullException(nameof(api)); _state = state ?? throw new ArgumentNullException(nameof(state)); }

        public IEnumerator Reload(Action<ProgressionResult> completed)
        {
            ApiResult<EconomySnapshot> economy = null; ApiResult<QuestSnapshot> quest = null;
            yield return _api.GetEconomy(value => economy = value);
            if (economy?.IsSuccess != true) { completed?.Invoke(Failed(economy?.Error)); yield break; }
            yield return _api.GetQuests(value => quest = value);
            if (quest?.IsSuccess != true) { completed?.Invoke(Failed(quest?.Error)); yield break; }
            _state.ApplyEconomy(economy.Data); _state.ApplyQuest(quest.Data);
            completed?.Invoke(new ProgressionResult { Outcome = ProgressionOutcome.Succeeded, Economy = economy.Data, Quest = quest.Data });
        }

        public IEnumerator ClaimDailyReward(Action<ProgressionResult> completed)
        {
            if (_state.Economy == null) { completed?.Invoke(Failed(Missing("economy"))); yield break; }
            ApiResult<EconomySnapshot> response = null;
            yield return _api.ClaimDailyReward(new RevisionRequest { expectedRevision = _state.Economy.revision }, value => response = value);
            if (response?.IsSuccess == true)
            {
                _state.ApplyEconomy(response.Data);
                completed?.Invoke(new ProgressionResult { Outcome = ProgressionOutcome.Succeeded, Economy = response.Data });
                yield break;
            }
            if (response?.Error?.Kind == ApiErrorKind.RevisionConflict) { yield return Resynchronize(response.Error, completed); yield break; }
            completed?.Invoke(Failed(response?.Error));
        }

        public IEnumerator ClaimQuest(QuestClaimIntent intent, Action<ProgressionResult> completed)
        {
            if (intent == null) throw new ArgumentNullException(nameof(intent));
            if (_state.Quest == null || _state.Economy == null) { completed?.Invoke(Failed(Missing("quest_and_economy"))); yield break; }
            ApiResult<QuestRewardResponse> response = null;
            yield return _api.ClaimQuestReward(intent.QuestId, new ClaimQuestRewardRequest
            {
                idempotencyKey = intent.IdempotencyKey,
                expectedQuestRevision = _state.Quest.revision,
                expectedEconomyRevision = _state.Economy.revision
            }, value => response = value);
            if (response?.IsSuccess == true)
            {
                _state.ApplyQuest(response.Data.quest); _state.ApplyEconomy(response.Data.economy);
                completed?.Invoke(new ProgressionResult
                {
                    Outcome = response.Data.replayed ? ProgressionOutcome.Replayed : ProgressionOutcome.Succeeded,
                    Quest = response.Data.quest, Economy = response.Data.economy
                });
                yield break;
            }
            if (response?.Error?.Kind == ApiErrorKind.RevisionConflict) { yield return Resynchronize(response.Error, completed); yield break; }
            // 전송 결과가 불명확하면 호출자가 같은 intent를 보존해 같은 키로만 재시도합니다.
            completed?.Invoke(Failed(response?.Error));
        }

        private IEnumerator Resynchronize(ApiError conflict, Action<ProgressionResult> completed)
        {
            ProgressionResult latest = null; yield return Reload(value => latest = value);
            if (latest?.Outcome != ProgressionOutcome.Succeeded) { completed?.Invoke(latest); yield break; }
            latest.Outcome = ProgressionOutcome.ConflictResynchronized; latest.Error = conflict; completed?.Invoke(latest);
        }
        private static ProgressionResult Failed(ApiError error) => new() { Outcome = ProgressionOutcome.Failed, Error = error ?? Missing("unknown") };
        private static ApiError Missing(string target) => new() { Kind = ApiErrorKind.Http, Code = "client_state_missing", Message = target };
    }
}
