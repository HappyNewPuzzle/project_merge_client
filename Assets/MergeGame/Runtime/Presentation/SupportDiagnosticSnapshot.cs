using MergeGame.Client.Api;

namespace MergeGame.Client.Presentation
{
    /// <summary>
    /// 고객 지원에 전달해도 되는 최소 진단 정보입니다.
    /// 서버 메시지와 요청 데이터는 개인정보나 자격 증명을 포함할 수 있으므로 의도적으로 제외합니다.
    /// </summary>
    public sealed class SupportDiagnosticSnapshot
    {
        public ApiErrorKind ErrorKind { get; }
        public long StatusCode { get; }
        public string TraceId { get; }

        private SupportDiagnosticSnapshot(ApiErrorKind errorKind, long statusCode, string traceId)
        {
            ErrorKind = errorKind;
            StatusCode = statusCode;
            TraceId = traceId ?? string.Empty;
        }

        /// <summary>오류가 없을 때도 호출 측이 null 검사를 반복하지 않도록 안전한 빈 스냅샷을 반환합니다.</summary>
        public static SupportDiagnosticSnapshot From(ApiError error) => error == null
            ? new SupportDiagnosticSnapshot(ApiErrorKind.None, 0, string.Empty)
            : new SupportDiagnosticSnapshot(error.Kind, error.StatusCode, error.TraceId);

        /// <summary>로그나 지원 문의에 붙일 수 있는 고정 형식이며 토큰과 서버 메시지는 절대 포함하지 않습니다.</summary>
        public string ToSupportText() => $"kind={ErrorKind}; status={StatusCode}; traceId={TraceId}";
    }
}
