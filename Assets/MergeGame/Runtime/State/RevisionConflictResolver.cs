using System;
using System.Collections;
using MergeGame.Client.Api;

namespace MergeGame.Client.State
{
    public enum MutationOutcome { Succeeded, ConflictResynchronized, Failed }
    public sealed class MutationResult<TMutation, TState>
    {
        public MutationOutcome Outcome { get; internal set; }
        public TMutation Mutation { get; internal set; }
        public TState LatestServerState { get; internal set; }
        public ApiError Error { get; internal set; }
    }

    /// <summary>409 발생 시 변경 요청은 반복하지 않고 최신 조회만 수행해 상위 계층의 재판단을 요구합니다.</summary>
    public sealed class RevisionConflictResolver
    {
        public IEnumerator Execute<TMutation, TState>(
            Func<Action<ApiResult<TMutation>>, IEnumerator> mutate,
            Func<Action<ApiResult<TState>>, IEnumerator> reload,
            Action<MutationResult<TMutation, TState>> completed)
        {
            ApiResult<TMutation> mutation = null;
            yield return mutate(value => mutation = value);
            if (mutation?.IsSuccess == true)
            {
                completed?.Invoke(new MutationResult<TMutation, TState> { Outcome = MutationOutcome.Succeeded, Mutation = mutation.Data });
                yield break;
            }
            if (mutation?.Error?.Kind != ApiErrorKind.RevisionConflict)
            {
                completed?.Invoke(new MutationResult<TMutation, TState> { Outcome = MutationOutcome.Failed, Error = mutation?.Error });
                yield break;
            }
            ApiResult<TState> latest = null;
            yield return reload(value => latest = value);
            completed?.Invoke(latest?.IsSuccess == true
                ? new MutationResult<TMutation, TState> { Outcome = MutationOutcome.ConflictResynchronized, LatestServerState = latest.Data, Error = mutation.Error }
                : new MutationResult<TMutation, TState> { Outcome = MutationOutcome.Failed, Error = latest?.Error ?? mutation.Error });
        }
    }
}
