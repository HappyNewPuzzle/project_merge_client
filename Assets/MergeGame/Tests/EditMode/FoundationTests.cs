using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.Configuration;
using NUnit.Framework;
using UnityEngine.Networking;

namespace MergeGame.Client.Tests.EditMode
{
    public sealed class FoundationTests
    {
        [Test] public void InMemoryStore_ReplacesRotatedSessionAtomically()
        {
            var store = new InMemoryTokenStore();
            store.SaveSession(new AuthSession("p", "a1", "", "r1", ""));
            store.SaveSession(new AuthSession("p", "a2", "", "r2", ""));
            Assert.That(store.LoadSession().AccessToken, Is.EqualTo("a2"));
            Assert.That(store.LoadSession().RefreshToken, Is.EqualTo("r2"));
        }
        [TestCase(401, "", ApiErrorKind.Unauthorized)]
        [TestCase(403, "account_suspended", ApiErrorKind.AccountSuspended)]
        [TestCase(409, "stale_revision", ApiErrorKind.RevisionConflict)]
        public void HttpErrors_AreClassified(long status, string code, ApiErrorKind expected)
        {
            var error = MergeGameApiClient.ClassifyError(UnityWebRequest.Result.ProtocolError, status, new ApiProblem { code = code }, "");
            Assert.That(error.Kind, Is.EqualTo(expected));
        }
        [Test] public void ConnectionError_IsNetworkError() =>
            Assert.That(MergeGameApiClient.ClassifyError(UnityWebRequest.Result.ConnectionError, 0, null, "offline").Kind, Is.EqualTo(ApiErrorKind.Network));
        [Test] public void EnvironmentCatalog_UsesHttpsAndContainsNoCredential()
        {
            var endpoint = ServerEndpointCatalog.For(ServerEnvironment.Production);
            Assert.That(endpoint.BaseUrl, Does.StartWith("https://"));
            Assert.That(endpoint.BaseUrl, Does.Not.Contain("@"));
        }
    }
}

