using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;
using HF.Shared;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;
using HF.Shared.Layout;
using HF.Shared.Pages;
using HF.Shared.Pages.Auth;
using HF.Shared.Pages.Courses;
using HF.Shared.Pages.Enrollments;
using HF.Shared.Pages.Students;
using HF.Shared.Pages.Subjects;
using HF.Shared.Pages.Teachers;
using HF.Shared.PageModels.Auth;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedAuthenticationAndSessionTests
{
    #region 1. Route Protection & Explicit Authorization Attributes

    [Fact]
    public void ProtectedPages_HaveExplicitAuthorizeAttribute()
    {
        var protectedPageTypes = new[]
        {
            typeof(Home),
            typeof(CourseList),
            typeof(CourseCreate),
            typeof(CourseDetails),
            typeof(CourseEdit),
            typeof(SubjectList),
            typeof(SubjectCreate),
            typeof(SubjectDetails),
            typeof(SubjectEdit),
            typeof(StudentList),
            typeof(StudentCreate),
            typeof(StudentDetails),
            typeof(StudentEdit),
            typeof(TeacherList),
            typeof(TeacherCreate),
            typeof(TeacherDetails),
            typeof(TeacherEdit),
            typeof(EnrollmentList),
            typeof(EnrollmentCreate),
            typeof(EnrollmentDetails),
            typeof(EnrollmentEdit),
            typeof(ManageProfile),
            typeof(ManageRoles),
            typeof(ManagePermissions)
        };

        foreach (var pageType in protectedPageTypes)
        {
            var authAttr = pageType.GetCustomAttribute<AuthorizeAttribute>(inherit: true);
            Assert.True(authAttr != null, $"Page '{pageType.Name}' must be decorated with [Authorize] so unauthenticated users cannot render or interact with it.");
        }
    }

    [Fact]
    public void PublicPages_HaveExplicitAllowAnonymousAttribute()
    {
        var publicPageTypes = new[]
        {
            typeof(Login),
            typeof(Register),
            typeof(ForgotPassword),
            typeof(ResetPassword),
            typeof(ConfirmEmail),
            typeof(NotFound)
        };

        foreach (var pageType in publicPageTypes)
        {
            var allowAnon = pageType.GetCustomAttribute<AllowAnonymousAttribute>(inherit: true);
            Assert.True(allowAnon != null, $"Page '{pageType.Name}' must be decorated with [AllowAnonymous] to remain accessible to anonymous users.");
        }
    }

    #endregion

    #region 2. JWT Parser & AuthenticationStateProvider

    [Fact]
    public void JwtParser_ExtractsClaimsAndExpirationAccurately()
    {
        var expSeconds = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var token = CreateSyntheticJwt("user-999", "teacher@horizon.org", new[] { "Teacher" }, expSeconds);

        var claims = JwtParser.ParseClaimsFromJwt(token).ToList();
        var expiration = JwtParser.GetExpiration(token);
        var isExpired = JwtParser.IsExpired(token);

        Assert.Contains(claims, c => c.Type == ClaimTypes.NameIdentifier && c.Value == "user-999");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Email && c.Value == "teacher@horizon.org");
        Assert.Contains(claims, c => c.Type == ClaimTypes.Role && c.Value == "Teacher");
        Assert.NotNull(expiration);
        Assert.False(isExpired);
    }

    [Fact]
    public async Task HorizonAuthenticationStateProvider_EmptyStore_ReturnsAnonymous()
    {
        var tokenStore = new InMemoryTokenStore();
        var services = new ServiceCollection().BuildServiceProvider();
        using var provider = new HorizonAuthenticationStateProvider(tokenStore, services);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.NotNull(state);
        Assert.False(state.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public async Task HorizonAuthenticationStateProvider_ValidToken_ReturnsAuthenticatedPrincipal()
    {
        var tokenStore = new InMemoryTokenStore();
        var expSeconds = DateTimeOffset.UtcNow.AddHours(2).ToUnixTimeSeconds();
        var jwt = CreateSyntheticJwt("user-100", "admin@horizon.org", new[] { "Admin" }, expSeconds);
        await tokenStore.SaveTokensAsync(jwt, "refresh-100");

        var services = new ServiceCollection().BuildServiceProvider();
        using var provider = new HorizonAuthenticationStateProvider(tokenStore, services);

        var state = await provider.GetAuthenticationStateAsync();

        Assert.NotNull(state);
        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("admin@horizon.org", state.User.FindFirst(ClaimTypes.Email)?.Value);
        Assert.True(state.User.IsInRole("Admin"));
    }

    [Fact]
    public async Task HorizonAuthenticationStateProvider_TokenChanges_NotifiesSubscribersReactively()
    {
        var tokenStore = new InMemoryTokenStore();
        var services = new ServiceCollection().BuildServiceProvider();
        using var provider = new HorizonAuthenticationStateProvider(tokenStore, services);

        int notificationsCount = 0;
        provider.AuthenticationStateChanged += _ => notificationsCount++;

        // 1. Simulate Login
        var expSeconds = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var jwt = CreateSyntheticJwt("user-1", "user@horizon.org", new[] { "Teacher" }, expSeconds);
        await tokenStore.SaveTokensAsync(jwt, "refresh-1");

        // Allow task dispatch
        await Task.Delay(50);
        Assert.True(notificationsCount >= 1);

        var stateAfterLogin = await provider.GetAuthenticationStateAsync();
        Assert.True(stateAfterLogin.User.Identity?.IsAuthenticated);

        // 2. Simulate Logout
        await tokenStore.ClearTokensAsync();
        await Task.Delay(50);
        Assert.True(notificationsCount >= 2);

        var stateAfterLogout = await provider.GetAuthenticationStateAsync();
        Assert.False(stateAfterLogout.User.Identity?.IsAuthenticated);
    }

    #endregion

    #region 3. Storage Adapters (Web Encrypted & Mobile Secure)

    private class MockJsRuntime : IJSRuntime
    {
        public Dictionary<string, string> Storage { get; } = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "localStorage.getItem")
            {
                var key = args?[0]?.ToString() ?? "";
                Storage.TryGetValue(key, out var val);
                return ValueTask.FromResult((TValue)(object)val!);
            }
            if (identifier == "localStorage.setItem")
            {
                var key = args?[0]?.ToString() ?? "";
                var val = args?[1]?.ToString() ?? "";
                Storage[key] = val;
                return ValueTask.FromResult(default(TValue)!);
            }
            if (identifier == "localStorage.removeItem")
            {
                var key = args?[0]?.ToString() ?? "";
                Storage.Remove(key);
                return ValueTask.FromResult(default(TValue)!);
            }

            return ValueTask.FromResult(default(TValue)!);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
            => InvokeAsync<TValue>(identifier, args);
    }

    [Fact]
    public async Task EncryptedBrowserTokenStore_NeverStoresPlaintext_AndDecryptsSuccessfully()
    {
        var mockJs = new MockJsRuntime();
        var store = new EncryptedBrowserTokenStore(mockJs);

        var rawAccessToken = "header.payload.signature-plain-secret";
        var rawRefreshToken = "plain-refresh-token-12345";

        await store.SaveTokensAsync(rawAccessToken, rawRefreshToken);

        // Verify storage entries exist
        Assert.True(mockJs.Storage.ContainsKey("hf_sec_at"));
        Assert.True(mockJs.Storage.ContainsKey("hf_sec_rt"));

        var storedAtValue = mockJs.Storage["hf_sec_at"];
        var storedRtValue = mockJs.Storage["hf_sec_rt"];

        // Guarantee: never cleartext
        Assert.DoesNotContain(rawAccessToken, storedAtValue);
        Assert.DoesNotContain(rawRefreshToken, storedRtValue);

        // Read back and verify decryption
        var recoveredAt = await store.GetAccessTokenAsync();
        var recoveredRt = await store.GetRefreshTokenAsync();

        Assert.Equal(rawAccessToken, recoveredAt);
        Assert.Equal(rawRefreshToken, recoveredRt);

        // Clear tokens
        await store.ClearTokensAsync();
        Assert.False(mockJs.Storage.ContainsKey("hf_sec_at"));
        Assert.False(mockJs.Storage.ContainsKey("hf_sec_rt"));
    }

    [Fact]
    public async Task EncryptedBrowserTokenStore_CorruptedOrTamperedData_ClearsSafely()
    {
        var mockJs = new MockJsRuntime();
        var store = new EncryptedBrowserTokenStore(mockJs);

        // Place tampered string in storage
        mockJs.Storage["hf_sec_at"] = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34, 35, 36, 37, 38, 39, 40, 41, 42, 43, 44, 45, 46, 47, 48, 49, 50 });

        var recoveredAt = await store.GetAccessTokenAsync();

        // Must return null safely and remove corrupted entry
        Assert.Null(recoveredAt);
        Assert.False(mockJs.Storage.ContainsKey("hf_sec_at"));
    }

    private class TestPersistentTokenStore : ITokenStore
    {
        private string? _at;
        private string? _rt;
        public event EventHandler? TokensChanged;

        public Task<string?> GetAccessTokenAsync() => Task.FromResult(_at);
        public Task<string?> GetRefreshTokenAsync() => Task.FromResult(_rt);
        public Task SaveTokensAsync(string accessToken, string? refreshToken)
        {
            _at = accessToken;
            _rt = refreshToken;
            TokensChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
        public Task ClearTokensAsync()
        {
            _at = null;
            _rt = null;
            TokensChanged?.Invoke(this, EventArgs.Empty);
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task PersistentTokenStore_PersistsAndClearsTokensWithFallback()
    {
        var store = new TestPersistentTokenStore();

        await store.SaveTokensAsync("access-sec-123", "refresh-sec-456");

        var at = await store.GetAccessTokenAsync();
        var rt = await store.GetRefreshTokenAsync();

        Assert.Equal("access-sec-123", at);
        Assert.Equal("refresh-sec-456", rt);

        await store.ClearTokensAsync();

        Assert.Null(await store.GetAccessTokenAsync());
        Assert.Null(await store.GetRefreshTokenAsync());
    }

    #endregion

    #region 4. Refresh & Token Rotation Tests (RefreshTokenHandler)

    [Fact]
    public async Task RefreshTokenHandler_Catches401_RefreshesToken_AndReplaysOriginalRequest()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("expired-access-token", "valid-refresh-token");

        int protectedEndpointCalls = 0;
        int refreshEndpointCalls = 0;

        var innerHandler = new MockHttpHandler(req =>
        {
            var uri = req.RequestUri!.ToString();

            // Refresh endpoint call
            if (uri.Contains("/api/v1/auth/refresh"))
            {
                refreshEndpointCalls++;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "tokenType": "Bearer",
                            "accessToken": "new-rotated-access-token",
                            "expiresIn": 3600,
                            "refreshToken": "new-rotated-refresh-token"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }

            // Protected endpoint call
            if (uri.Contains("/api/v1/courses"))
            {
                protectedEndpointCalls++;
                var authHeader = req.Headers.Authorization?.ToString();

                // First call with expired token returns 401
                if (authHeader == "Bearer expired-access-token")
                {
                    return new HttpResponseMessage(HttpStatusCode.Unauthorized);
                }

                // Replayed call with rotated token returns 200 OK
                if (authHeader == "Bearer new-rotated-access-token")
                {
                    return new HttpResponseMessage(HttpStatusCode.OK)
                    {
                        Content = new StringContent("[]", Encoding.UTF8, "application/json")
                    };
                }
            }

            return new HttpResponseMessage(HttpStatusCode.NotFound);
        });

        var refreshHandler = new RefreshTokenHandler(tokenStore, () => "http://localhost:8080")
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(refreshHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/api/v1/courses");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-access-token");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, refreshEndpointCalls);
        Assert.Equal(2, protectedEndpointCalls);

        // Verify rotated tokens were saved in store
        Assert.Equal("new-rotated-access-token", await tokenStore.GetAccessTokenAsync());
        Assert.Equal("new-rotated-refresh-token", await tokenStore.GetRefreshTokenAsync());
    }

    [Fact]
    public async Task RefreshTokenHandler_DefiniteRefreshFailure_ClearsTokensAndReturns401()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("expired-access-token", "revoked-refresh-token");

        var innerHandler = new MockHttpHandler(req =>
        {
            var uri = req.RequestUri!.ToString();
            if (uri.Contains("/api/v1/auth/refresh"))
            {
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    Content = new StringContent("{\"success\":false,\"message\":\"Invalid refresh token.\"}", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        });

        var refreshHandler = new RefreshTokenHandler(tokenStore, () => "http://localhost:8080")
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(refreshHandler);
        using var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/api/v1/courses");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-access-token");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        // Session must be cleared on definite refresh failure
        Assert.Null(await tokenStore.GetAccessTokenAsync());
        Assert.Null(await tokenStore.GetRefreshTokenAsync());
    }

    [Fact]
    public async Task RefreshTokenHandler_ConcurrentRequests_ExecutesOnlyOneRefreshCall()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("expired-token", "refresh-token-concurrency");

        int refreshCalls = 0;

        var innerHandler = new MockHttpHandler(req =>
        {
            var uri = req.RequestUri!.ToString();
            if (uri.Contains("/api/v1/auth/refresh"))
            {
                Interlocked.Increment(ref refreshCalls);
                Thread.Sleep(50); // Simulate network latency
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "tokenType": "Bearer",
                            "accessToken": "single-rotated-token",
                            "expiresIn": 3600,
                            "refreshToken": "new-refresh-token"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }

            if (req.Headers.Authorization?.Parameter == "single-rotated-token")
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("{}", Encoding.UTF8, "application/json")
                };
            }

            return new HttpResponseMessage(HttpStatusCode.Unauthorized);
        });

        var refreshHandler = new RefreshTokenHandler(tokenStore, () => "http://localhost:8080")
        {
            InnerHandler = innerHandler
        };

        var client = new HttpClient(refreshHandler);

        // Execute 5 concurrent requests that all receive 401 initially
        var tasks = Enumerable.Range(1, 5).Select(_ =>
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "http://localhost:8080/api/v1/subjects");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "expired-token");
            return client.SendAsync(req);
        });

        var responses = await Task.WhenAll(tasks);

        foreach (var r in responses)
        {
            Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        }

        // Only ONE refresh operation must have been executed
        Assert.Equal(1, refreshCalls);
    }

    #endregion

    #region 5. Dependency Injection Registration Tests

    [Fact]
    public void AddHorizonClient_RegistersAuthInfrastructure_WithoutInMemoryStoreTakingPrecedenceInHosts()
    {
        var services = new ServiceCollection();

        // Host registers custom persistent store first
        services.AddScoped<ITokenStore, TestPersistentTokenStore>();

        services.AddHorizonClient("http://localhost:8080");

        var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        var resolvedTokenStore = sp.GetRequiredService<ITokenStore>();
        Assert.IsType<TestPersistentTokenStore>(resolvedTokenStore);

        var authStateProvider = sp.GetRequiredService<AuthenticationStateProvider>();
        Assert.IsType<HorizonAuthenticationStateProvider>(authStateProvider);

        var kiotaAuthProvider = sp.GetRequiredService<Microsoft.Kiota.Abstractions.Authentication.IAuthenticationProvider>();
        Assert.IsType<KiotaAuthenticationProvider>(kiotaAuthProvider);
    }

    #endregion

    #region 6. User Access, Dynamic Policy Provider & Login Flow Tests

    [Fact]
    public async Task HorizonAuthorizationPolicyProvider_DynamicallyGeneratesPolicy_RequiresPermissionOrAdmin()
    {
        var options = Microsoft.Extensions.Options.Options.Create(new AuthorizationOptions());
        var policyProvider = new HorizonAuthorizationPolicyProvider(options);

        var policy = await policyProvider.GetPolicyAsync("Enrollments.Create");
        Assert.NotNull(policy);

        // Evaluate requirement against User with permission claim
        var userWithPermission = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("permission", "Enrollments.Create")
        }, "TestAuth"));

        // Evaluate requirement against User with Admin role
        var userWithAdmin = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }, "TestAuth"));

        // Evaluate requirement against unprivileged user
        var unprivilegedUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Student")
        }, "TestAuth"));

        var assertionReq = Assert.Single(policy.Requirements.OfType<Microsoft.AspNetCore.Authorization.Infrastructure.AssertionRequirement>());

        var successContext = new AuthorizationHandlerContext(new[] { assertionReq }, userWithPermission, null);
        var successResult = await assertionReq.Handler(successContext);
        Assert.True(successResult);

        var adminContext = new AuthorizationHandlerContext(new[] { assertionReq }, userWithAdmin, null);
        var adminResult = await assertionReq.Handler(adminContext);
        Assert.True(adminResult);

        var failureContext = new AuthorizationHandlerContext(new[] { assertionReq }, unprivilegedUser, null);
        var failureResult = await assertionReq.Handler(failureContext);
        Assert.False(failureResult);
    }

    [Fact]
    public async Task UserAccessService_LoadsAndCachesAccess_AndClearsSessionOn401()
    {
        var fakeAuth = new FakeAuthService();
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("valid-token", "valid-refresh");

        var userAccess = new UserAccessService(fakeAuth, tokenStore);

        // 1. Initial refresh success
        var success = await userAccess.LoadCurrentAccessAsync();
        Assert.True(success);
        Assert.Contains("Teacher", userAccess.CurrentRoles);
        Assert.Contains("Enrollments.Read", userAccess.CurrentPolicies);

        // 2. Refresh fails with 401 Unauthorized -> must clear session & cache
        fakeAuth.ShouldSucceed = false;
        fakeAuth.FailureStatusCode = 401;

        var fail401 = await userAccess.LoadCurrentAccessAsync();
        Assert.False(fail401);
        Assert.Empty(userAccess.CurrentRoles);
        Assert.Empty(userAccess.CurrentPolicies);
        Assert.Null(await tokenStore.GetAccessTokenAsync());
        Assert.Null(await tokenStore.GetRefreshTokenAsync());
    }

    [Fact]
    public async Task LoginPageModel_LoginAsync_ReentrancyGuard_PreventsDoubleSubmission()
    {
        var fakeAuth = new FakeAuthService();
        var tokenStore = new InMemoryTokenStore();
        var userAccess = new UserAccessService(fakeAuth, tokenStore);
        var model = new LoginPageModel(fakeAuth, userAccess)
        {
            Email = "test@horizon.org",
            Password = "Password123!"
        };

        // When IsBusy is true (first submission in flight), subsequent call must immediately return false
        model.IsBusy = true;
        var result = await model.LoginAsync();

        Assert.False(result);
    }

    #endregion

    #region Helpers

    private static string CreateSyntheticJwt(string userId, string email, IEnumerable<string> roles, long expSeconds)
    {
        var header = Convert.ToBase64String(Encoding.UTF8.GetBytes("{\"alg\":\"HS256\",\"typ\":\"JWT\"}")).TrimEnd('=');

        var payloadDict = new Dictionary<string, object>
        {
            ["sub"] = userId,
            ["email"] = email,
            ["roles"] = roles.ToArray(),
            ["exp"] = expSeconds
        };

        var payloadJson = JsonSerializer.Serialize(payloadDict);
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(payloadJson)).TrimEnd('=');
        var signature = Convert.ToBase64String(Encoding.UTF8.GetBytes("synthetic-signature")).TrimEnd('=');

        return $"{header}.{payload}.{signature}";
    }

    private class MockHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public MockHttpHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }

    #endregion
}
