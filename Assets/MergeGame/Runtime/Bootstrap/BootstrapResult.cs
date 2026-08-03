using MergeGame.Client.Api;
namespace MergeGame.Client.Bootstrap
{
    public enum BootstrapStatus { Completed, Failed }
    /// <summary>화면 진입 전에 필요한 서버 권위 상태의 초기 스냅샷입니다.</summary>
    public sealed class InitialGameState
    {
        public BoardState Board { get; internal set; } public EconomySnapshot Economy { get; internal set; }
        public QuestSnapshot Quest { get; internal set; } public SocialProfileSnapshot SocialProfile { get; internal set; }
    }
    public sealed class BootstrapResult
    {
        public BootstrapStatus Status { get; internal set; } public InitialGameState State { get; internal set; }
        public ApiError Error { get; internal set; } public bool IsCompleted => Status == BootstrapStatus.Completed;
    }
}

