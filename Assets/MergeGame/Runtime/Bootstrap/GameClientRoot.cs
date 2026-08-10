using System.Collections;
using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.Gameplay.Board;
using MergeGame.Client.Gameplay.Progression;
using MergeGame.Client.Gameplay.Social;
using MergeGame.Client.Presentation;
using UnityEngine;

namespace MergeGame.Client.Bootstrap
{
    /// <summary>Scene의 HUD와 인증·게임 명령 계층을 연결하는 유일한 composition root입니다.</summary>
    [RequireComponent(typeof(GameHudPresenter))]
    public sealed class GameClientRoot : MonoBehaviour
    {
        [SerializeField] private bool autoStart = true;
        private GameClientContext _context; private GameHudPresenter _view; private GameUiModel _model;
        private QuestClaimIntent _claimIntent;
        private void Awake()
        {
            _view = GetComponent<GameHudPresenter>(); _model = new GameUiModel();
            _view.GenerateRequested += slot => StartCoroutine(RunBoard(_context.Board.Generate(slot, OnBoard)));
            _view.MergeRequested += (source, target) => StartCoroutine(RunBoard(_context.Board.Merge(source, target, OnBoard)));
            _view.DailyRewardRequested += () => StartCoroutine(_context.Progression.ClaimDailyReward(OnProgression));
            _view.QuestClaimRequested += ClaimQuest;
            _view.AddFriendRequested += code => StartCoroutine(_context.Social.AddFriend(code, OnSocial));
            if (autoStart) StartCoroutine(StartClient());
        }
        private IEnumerator StartClient()
        {
#if UNITY_EDITOR
            _context = GameClientContextFactory.Create(Configuration.ServerEndpointCatalog.Current.BaseUrl, new InMemoryTokenStore());
#else
            _context = GameClientContextFactory.CreateForPlayer();
#endif
            BootstrapResult result = null; yield return _context.Bootstrapper.Run(value => result = value);
            if (result?.IsCompleted != true) { ShowError(result?.Error); yield break; }
            SocialCommandResult social = null; yield return _context.Social.Reload(value => social = value);
            if (social?.Outcome == SocialOutcome.Failed) { ShowError(social.Error); yield break; }
            Render();
        }
        private IEnumerator RunBoard(IEnumerator operation) { yield return operation; }
        private void OnBoard(BoardCommandResult result)
        { if (result.Outcome == BoardCommandOutcome.Failed) ShowError(result.Error); else if (result.Outcome == BoardCommandOutcome.ConflictResynchronized) ShowConflict(result.Error); else Render(); }
        private void OnProgression(ProgressionResult result)
        { if (result.Outcome == ProgressionOutcome.Failed) ShowError(result.Error); else if (result.Outcome == ProgressionOutcome.ConflictResynchronized) ShowConflict(result.Error); else { _claimIntent = null; Render(); } }
        private void OnSocial(SocialCommandResult result) { if (result.Outcome == SocialOutcome.Failed) ShowError(result.Error); else Render(); }
        private void ClaimQuest()
        {
            if (_context.State.Quest == null) return;
            _claimIntent ??= QuestClaimIntent.Create(_context.State.Quest.questId);
            StartCoroutine(_context.Progression.ClaimQuest(_claimIntent, OnProgression));
        }
        private void Render() { _model.Apply(_context.State); _view.Render(_model); }
        private void ShowError(ApiError error) { _model.ApplyError(error); _view.Render(_model); }
        private void ShowConflict(ApiError error) { _model.ApplyError(error); _view.Render(_model); }
    }
}

