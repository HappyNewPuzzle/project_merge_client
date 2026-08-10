using MergeGame.Client.Api;
using MergeGame.Client.Authentication;
using MergeGame.Client.Configuration;
using NUnit.Framework;
using UnityEngine.Networking;
using System.Collections.Generic;

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
        [Test] public void SecureTokenStore_RoundTripsThroughPlatformBoundary()
        {
            var secrets = new FakeSecrets(); var store = new SecureTokenStore(secrets);
            store.SaveGuestCredential(new GuestCredential("player", "guest-secret"));
            store.SaveSession(new AuthSession("player", "access-secret", "a-exp", "refresh-secret", "r-exp"));
            Assert.That(store.LoadGuestCredential().PlayerId, Is.EqualTo("player"));
            Assert.That(store.LoadSession().RefreshToken, Is.EqualTo("refresh-secret"));
            store.ClearSession();
            Assert.That(store.LoadSession(), Is.Null);
        }
        private sealed class FakeSecrets : IPlatformSecretStore
        {
            private readonly Dictionary<string, string> _values = new();
            public string Get(string key) => _values.TryGetValue(key, out var value) ? value : null;
            public void Set(string key, string value) => _values[key] = value;
            public void Delete(string key) => _values.Remove(key);
        }
    }
}
