using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace MergeGame.Client.Api
{
    /// <summary>서버의 공개 `/api/v1` 게임 계약만 호출하는 UnityWebRequest 클라이언트입니다.</summary>
    public sealed class MergeGameApiClient : IMergeGameApiClient
    {
        private const string ApiPrefix = "/api/v1";
        private readonly string _baseUrl;
        public string AccessToken { get; set; } = "";

        public MergeGameApiClient(string baseUrl)
        {
            if (string.IsNullOrWhiteSpace(baseUrl)) throw new ArgumentException("서버 주소가 필요합니다.", nameof(baseUrl));
            _baseUrl = baseUrl.TrimEnd('/');
        }

        public IEnumerator CreateGuest(Action<ApiResult<CreateGuestPlayerResponse>> done) => Send<CreateGuestPlayerResponse>("POST", "/players/guest", null, false, done);
        public IEnumerator LoginGuest(GuestLoginRequest body, Action<ApiResult<GuestLoginResponse>> done) => Send<GuestLoginResponse>("POST", "/auth/guest", body, false, done);
        public IEnumerator RefreshAccessToken(RefreshTokenRequest body, Action<ApiResult<GuestLoginResponse>> done) => Send<GuestLoginResponse>("POST", "/auth/refresh", body, false, done);
        public IEnumerator Logout(RefreshTokenRequest body, Action<ApiResult<EmptyResponse>> done) => Send<EmptyResponse>("POST", "/auth/logout", body, true, done);
        public IEnumerator GetCurrentPlayer(Action<ApiResult<CurrentPlayerResponse>> done) => Send<CurrentPlayerResponse>("GET", "/players/me", null, true, done);
        public IEnumerator InitializeBoard(Action<ApiResult<BoardState>> done) => Send<BoardState>("POST", "/board/", null, true, done);
        public IEnumerator GetBoard(Action<ApiResult<BoardState>> done) => Send<BoardState>("GET", "/board/", null, true, done);
        public IEnumerator MergeItems(MergeBoardItemsRequest body, Action<ApiResult<BoardState>> done) => Send<BoardState>("POST", "/board/merge", body, true, done);
        public IEnumerator InitializeEconomy(Action<ApiResult<EconomySnapshot>> done) => Send<EconomySnapshot>("POST", "/economy/", null, true, done);
        public IEnumerator GetEconomy(Action<ApiResult<EconomySnapshot>> done) => Send<EconomySnapshot>("GET", "/economy/", null, true, done);
        public IEnumerator GenerateItem(GenerateItemRequest body, Action<ApiResult<GenerateItemResponse>> done) => Send<GenerateItemResponse>("POST", "/economy/generate", body, true, done);
        public IEnumerator ClaimDailyReward(RevisionRequest body, Action<ApiResult<EconomySnapshot>> done) => Send<EconomySnapshot>("POST", "/economy/daily-reward", body, true, done);
        public IEnumerator InitializeQuests(Action<ApiResult<QuestSnapshot>> done) => Send<QuestSnapshot>("POST", "/quests/", null, true, done);
        public IEnumerator GetQuests(Action<ApiResult<QuestSnapshot>> done) => Send<QuestSnapshot>("GET", "/quests/", null, true, done);
        public IEnumerator ClaimQuestReward(string questId, ClaimQuestRewardRequest body, Action<ApiResult<QuestRewardResponse>> done) =>
            Send<QuestRewardResponse>("POST", "/quests/" + UnityWebRequest.EscapeURL(questId) + "/claim", body, true, done);
        public IEnumerator InitializeSocialProfile(Action<ApiResult<SocialProfileSnapshot>> done) => Send<SocialProfileSnapshot>("POST", "/social/profile", null, true, done);
        public IEnumerator GetSocialProfile(Action<ApiResult<SocialState>> done) => Send<SocialState>("GET", "/social/profile", null, true, done);
        public IEnumerator AddFriend(AddFriendRequest body, Action<ApiResult<AddFriendResponse>> done) => Send<AddFriendResponse>("POST", "/social/friends", body, true, done);
        public IEnumerator SendFriendEnergyGift(string playerId, Action<ApiResult<EnergyGiftResponse>> done) =>
            Send<EnergyGiftResponse>("POST", "/social/friends/" + UnityWebRequest.EscapeURL(playerId) + "/energy-gift", null, true, done);

        private IEnumerator Send<T>(string method, string path, object body, bool authenticated, Action<ApiResult<T>> completed)
        {
            var json = body == null ? "{}" : JsonUtility.ToJson(body);
            using var request = new UnityWebRequest(_baseUrl + ApiPrefix + path, method);
            if (method != UnityWebRequest.kHttpVerbGET)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("X-Trace-Id", Guid.NewGuid().ToString("N"));
            if (authenticated && !string.IsNullOrWhiteSpace(AccessToken)) request.SetRequestHeader("Authorization", "Bearer " + AccessToken);

            yield return request.SendWebRequest();
            var raw = request.downloadHandler?.text ?? "";
            var success = request.responseCode is >= 200 and < 300;
            var result = new ApiResult<T> { IsSuccess = success, StatusCode = request.responseCode };
            if (success)
            {
                if (!string.IsNullOrWhiteSpace(raw)) result.Data = JsonUtility.FromJson<T>(raw);
            }
            else
            {
                ApiProblem problem = null;
                if (!string.IsNullOrWhiteSpace(raw)) problem = JsonUtility.FromJson<ApiProblem>(raw);
                result.Problem = problem;
                result.Error = ClassifyError(request.result, request.responseCode, problem, request.error);
            }
            completed?.Invoke(result);
        }

        /// <summary>HTTP 본문 형식에 의존하지 않고 필수 복구 분기를 일관되게 분류합니다.</summary>
        public static ApiError ClassifyError(UnityWebRequest.Result requestResult, long statusCode, ApiProblem problem, string transportMessage)
        {
            var code = problem?.code ?? "";
            var kind = requestResult == UnityWebRequest.Result.ConnectionError ? ApiErrorKind.Network
                : statusCode == 401 ? ApiErrorKind.Unauthorized
                : statusCode == 403 && code == "account_suspended" ? ApiErrorKind.AccountSuspended
                : statusCode == 409 ? ApiErrorKind.RevisionConflict
                : ApiErrorKind.Http;
            return new ApiError { Kind = kind, StatusCode = statusCode, Code = code, Message = problem?.message ?? problem?.detail ?? transportMessage ?? "", TraceId = problem?.traceId ?? "" };
        }
    }
}
