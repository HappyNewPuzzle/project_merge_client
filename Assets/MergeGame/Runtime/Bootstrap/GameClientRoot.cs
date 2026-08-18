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
        private bool _busy;
        private void Awake()
        {
            _view = GetComponent<GameHudPresenter>(); _model = new GameUiModel();
            _view.GenerateRequested += GenerateFromGenerator;
            _view.MergeRequested += (source, target) => StartExclusive(_context.Board.Merge(source, target, OnBoard));
            _view.DailyRewardRequested += () => StartExclusive(_context.Progression.ClaimDailyReward(OnProgression));
            _view.QuestClaimRequested += ClaimQuest;
            _view.AddFriendRequested += code => StartExclusive(_context.Social.AddFriend(code, OnSocial));
            _view.EnergyGiftRequested += id => StartExclusive(_context.Social.SendEnergyGift(id, OnSocial));
            _view.RetryRequested += () => { if (!_busy) StartCoroutine(StartClient()); };
            _view.LogoutRequested += () => StartCoroutine(Logout());
            if (autoStart) StartCoroutine(StartClient());
        }
        private IEnumerator StartClient()
        {
#if UNITY_EDITOR
#if MERGEGAME_USE_LIVE_SERVER
            _context = GameClientContextFactory.Create(Configuration.ServerEndpointCatalog.Current.BaseUrl, new InMemoryTokenStore());
#else
            // Editor 기본값은 완전 오프라인입니다. 실제 서버 통합이 필요할 때만 명시적으로 define을 켭니다.
            _context = GameClientContextFactory.CreateOffline();
#endif
#else
            _context = GameClientContextFactory.CreateForPlayer();
#endif
            BootstrapResult result = null; yield return _context.Bootstrapper.Run(value => result = value);
            if (result?.IsCompleted != true) { ShowError(result?.Error); yield break; }
            SocialCommandResult social = null; yield return _context.Social.Reload(value => social = value);
            if (social?.Outcome == SocialOutcome.Failed) { ShowError(social.Error); yield break; }
            Render();
        }
        private void StartExclusive(IEnumerator operation)
        {
            if (_busy || _context == null) return;
            _busy = true; _view.SetInteractionEnabled(false); StartCoroutine(operation);
        }
        private void GenerateFromGenerator()
        {
            if (_context?.State?.Economy == null || _model == null) return;
            if (_context.State.Economy.energy <= 0) { _view.SetStatus("에너지가 부족합니다."); return; }
            var targetSlot = BoardGeneratorPlacement.FindFirstEmpty(_model.Slots);
            if (targetSlot < 0) { _view.SetStatus("보드에 빈 칸이 없습니다."); return; }
            // 기존 서버 계약에서는 위치가 필요하지만 실제 생성·에너지·revision은 서버 응답으로만 확정합니다.
            StartExclusive(_context.Board.Generate(targetSlot, OnBoard));
        }
        private void EndExclusive() { _busy = false; _view.SetInteractionEnabled(true); }
        private void OnBoard(BoardCommandResult result)
        { EndExclusive(); if (result.Outcome == BoardCommandOutcome.Failed) ShowError(result.Error); else if (result.Outcome == BoardCommandOutcome.ConflictResynchronized) ShowConflict(result.Error); else Render(); }
        private void OnProgression(ProgressionResult result)
        { EndExclusive(); if (result.Outcome == ProgressionOutcome.Failed) ShowError(result.Error); else if (result.Outcome == ProgressionOutcome.ConflictResynchronized) ShowConflict(result.Error); else { _claimIntent = null; Render(); } }
        private void OnSocial(SocialCommandResult result) { EndExclusive(); if (result.Outcome == SocialOutcome.Failed) ShowError(result.Error); else Render(); }
        private void ClaimQuest()
        {
            if (_context.State.Quest == null) return;
            _claimIntent ??= QuestClaimIntent.Create(_context.State.Quest.questId);
            StartExclusive(_context.Progression.ClaimQuest(_claimIntent, OnProgression));
        }
        private void Render() { _model.Apply(_context.State); _view.Render(_model); }
        private void ShowError(ApiError error) { _model.ApplyError(error); _view.Render(_model); }
        private void ShowConflict(ApiError error) { _model.ApplyError(error); _view.Render(_model); }
        private IEnumerator Logout()
        {
            if (_busy || _context == null) yield break;
            var session = _context.Tokens.LoadSession();
            if (session != null)
            {
                ApiResult<EmptyResponse> result = null;
                yield return _context.Api.Logout(new RefreshTokenRequest { refreshToken = session.RefreshToken }, value => result = value);
                if (result?.IsSuccess != true) { ShowError(result?.Error); yield break; }
            }
            _context.Tokens.ClearSession();
            _context.Api.AccessToken = "";
            StartCoroutine(StartClient());
        }
    }
}
