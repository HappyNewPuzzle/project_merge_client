namespace MergeGame.Client.Api
{
    /// <summary>화면과 재시도 정책이 전송 실패를 안전하게 구분하도록 정규화한 오류 종류입니다.</summary>
    public enum ApiErrorKind { None, Network, Unauthorized, AccountSuspended, RevisionConflict, Http }

    public sealed class ApiError
    {
        public ApiErrorKind Kind { get; internal set; }
        public long StatusCode { get; internal set; }
        public string Code { get; internal set; } = "";
        public string Message { get; internal set; } = "";
        public string TraceId { get; internal set; } = "";
    }

    /// <summary>원문 토큰을 노출하지 않고 성공 데이터와 분류된 오류만 전달합니다.</summary>
    public sealed class ApiResult<T>
    {
        public bool IsSuccess { get; internal set; }
        public long StatusCode { get; internal set; }
        public T Data { get; internal set; }
        public ApiProblem Problem { get; internal set; }
        public ApiError Error { get; internal set; }

        public static ApiResult<T> Success(T data, long statusCode = 200) => new() { IsSuccess = true, Data = data, StatusCode = statusCode };
        public static ApiResult<T> Failure(ApiErrorKind kind, long statusCode = 0, string code = "") =>
            new() { Error = new ApiError { Kind = kind, StatusCode = statusCode, Code = code }, StatusCode = statusCode };
    }
}

