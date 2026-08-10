using System;
using System.Collections.Generic;

namespace MergeGame.Client.Api
{
    public readonly struct ApiObservation
    {
        public long StatusCode { get; } public ApiErrorKind ErrorKind { get; } public long DurationMilliseconds { get; } public string TraceId { get; }
        public ApiObservation(long status, ApiErrorKind kind, long duration, string traceId)
        { StatusCode = status; ErrorKind = kind; DurationMilliseconds = duration; TraceId = traceId ?? ""; }
    }

    /// <summary>본문·URL·token 없이 최근 요청의 상태, 지연과 trace ID만 메모리에 보관합니다.</summary>
    public sealed class ClientDiagnostics
    {
        private readonly Queue<ApiObservation> _items = new(); private readonly int _capacity;
        public ClientDiagnostics(int capacity = 50) { if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity)); _capacity = capacity; }
        public IReadOnlyCollection<ApiObservation> Items => _items;
        public void Record(ApiObservation item) { while (_items.Count >= _capacity) _items.Dequeue(); _items.Enqueue(item); }
    }

    /// <summary>결과 불명 변경을 자동 반복하지 않도록 읽기와 명시적 멱등 요청만 재시도 대상으로 판정합니다.</summary>
    public static class NetworkRetryPolicy
    {
        public static bool CanRetry(ApiError error, bool isReadOnly, bool hasStableIdempotencyKey, int attempt) =>
            error?.Kind == ApiErrorKind.Network && attempt < 2 && (isReadOnly || hasStableIdempotencyKey);
    }
}

