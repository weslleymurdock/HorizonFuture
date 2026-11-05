using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HF.Client;
using HF.Shared.Auth;
using HF.Shared.Auth.Models;
using Microsoft.Kiota.Abstractions;
using Microsoft.Kiota.Http.HttpClientLibrary;
using Xunit;

namespace HF.UITests.Shared;

public class SharedAuthKiotaIntegrationTests
{
    private class TestHandler : HttpMessageHandler
    {
        public Func<HttpRequestMessage, HttpResponseMessage> Handler { get; set; } =
            _ => new HttpResponseMessage(HttpStatusCode.OK);

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(Handler(request));
        }
    }

    [Fact]
    public async Task KiotaAuthenticationProvider_AddsBearerHeader_WhenTokenExists()
    {
        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("test-jwt-token-123", "refresh-token-456");

        var provider = new KiotaAuthenticationProvider(tokenStore);
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/api/test"
        };

        await provider.AuthenticateRequestAsync(requestInfo);

        Assert.True(requestInfo.Headers.ContainsKey("Authorization"));
        Assert.Contains("Bearer test-jwt-token-123", requestInfo.Headers["Authorization"]);
    }

    [Fact]
    public async Task KiotaAuthenticationProvider_DoesNotAddBearerHeader_WhenTokenIsEmpty()
    {
        var tokenStore = new InMemoryTokenStore();
        var provider = new KiotaAuthenticationProvider(tokenStore);
        var requestInfo = new RequestInformation
        {
            HttpMethod = Method.GET,
            UrlTemplate = "http://localhost/api/test"
        };

        await provider.AuthenticateRequestAsync(requestInfo);

        Assert.False(requestInfo.Headers.ContainsKey("Authorization"));
    }

    [Fact]
    public async Task AuthService_LoginAsync_CallsKiotaClient_AndSavesTokens()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/login", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "tokenType": "Bearer",
                            "accessToken": "access-12345",
                            "expiresIn": 3600,
                            "refreshToken": "refresh-67890"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:5268"
        };
        var apiClient = new ApiClient(adapter);
        var authService = new AuthService(apiClient, tokenStore);

        var result = await authService.LoginAsync(new LoginDto("test@horizon.org", "Pass123!"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("access-12345", result.Data.AccessToken);
        Assert.Equal("access-12345", await tokenStore.GetAccessTokenAsync());
        Assert.Equal("refresh-67890", await tokenStore.GetRefreshTokenAsync());
    }

    [Fact]
    public async Task AuthService_LoginAsync_TranslatesKiota401Exception()
    {
        var testHandler = new TestHandler
        {
            Handler = req => new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = new StringContent(
                    """
                    {
                        "message": "Credenciais incorretas",
                        "success": false
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:5268"
        };
        var apiClient = new ApiClient(adapter);
        var authService = new AuthService(apiClient, tokenStore);

        var result = await authService.LoginAsync(new LoginDto("test@horizon.org", "WrongPass"));

        Assert.False(result.Succeeded);
        Assert.Contains("Credenciais incorretas", result.Message);
        Assert.Equal(401, result.StatusCode);
    }

    [Fact]
    public async Task AuthService_RegisterAsync_TranslatesKiota400IdentityResultException()
    {
        var testHandler = new TestHandler
        {
            Handler = req => new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    """
                    {
                        "succeeded": false,
                        "errors": [
                            "PasswordTooShort",
                            "PasswordRequiresDigit"
                        ]
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            }
        };

        var tokenStore = new InMemoryTokenStore();
        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:5268"
        };
        var apiClient = new ApiClient(adapter);
        var authService = new AuthService(apiClient, tokenStore);

        var result = await authService.RegisterAsync(new RegisterDto("test@horizon.org", "short"));

        Assert.False(result.Succeeded);
        Assert.Contains("PasswordTooShort", result.Message);
        Assert.Equal(2, result.Errors.Count);
        Assert.Equal(400, result.StatusCode);
    }

    [Fact]
    public async Task AuthService_RevokeTokenAsync_CallsKiota_AndClearsTokenStore()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/revoke", req.RequestUri!.ToString());
                return new HttpResponseMessage(HttpStatusCode.OK);
            }
        };

        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("to-revoke", "refresh");

        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:5268"
        };
        var apiClient = new ApiClient(adapter);
        var authService = new AuthService(apiClient, tokenStore);

        var result = await authService.RevokeTokenAsync();

        Assert.True(result.Succeeded);
        Assert.Null(await tokenStore.GetAccessTokenAsync());
        Assert.Null(await tokenStore.GetRefreshTokenAsync());
    }
}

