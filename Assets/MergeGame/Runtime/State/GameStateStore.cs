using MergeGame.Client.Api;
using MergeGame.Client.Bootstrap;

namespace MergeGame.Client.State
{
    /// <summary>서버가 반환한 스냅샷만 보관하며 재화나 revision을 로컬에서 증가시키지 않습니다.</summary>
    public interface IGameStateStore
    {
        BoardState Board { get; } EconomySnapshot Economy { get; } QuestSnapshot Quest { get; }
        SocialProfileSnapshot SocialProfile { get; }
        SocialState Social { get; }
        void Apply(InitialGameState state);
        void ApplyBoard(BoardState state); void ApplyEconomy(EconomySnapshot state); void ApplyQuest(QuestSnapshot state);
        void ApplySocial(SocialState state);
    }

    public sealed class GameStateStore : IGameStateStore
    {
        public BoardState Board { get; private set; } public EconomySnapshot Economy { get; private set; }
        public QuestSnapshot Quest { get; private set; } public SocialProfileSnapshot SocialProfile { get; private set; }
        public SocialState Social { get; private set; }
        public void Apply(InitialGameState state)
        {
            if (state == null) return;
            Board = state.Board; Economy = state.Economy; Quest = state.Quest; SocialProfile = state.SocialProfile;
        }
        public void ApplyBoard(BoardState state) { if (state != null) Board = state; }
        public void ApplyEconomy(EconomySnapshot state) { if (state != null) Economy = state; }
        public void ApplyQuest(QuestSnapshot state) { if (state != null) Quest = state; }
        public void ApplySocial(SocialState state) { if (state != null) Social = state; }
    }
}
