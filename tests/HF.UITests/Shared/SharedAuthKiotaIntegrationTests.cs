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

    [Fact]
    public async Task AuthService_RefreshTokenAsync_CallsKiota_AndUpdatesTokenStore()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/refresh", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "tokenType": "Bearer",
                            "accessToken": "new-access-555",
                            "expiresIn": 3600,
                            "refreshToken": "new-refresh-999"
                        }
                        """,
                        Encoding.UTF8,
                        "application/json")
                };
            }
        };

        var tokenStore = new InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("old-access", "old-refresh");

        var authProvider = new KiotaAuthenticationProvider(tokenStore);
        var adapter = new HttpClientRequestAdapter(authProvider, httpClient: new HttpClient(testHandler))
        {
            BaseUrl = "http://localhost:5268"
        };
        var apiClient = new ApiClient(adapter);
        var authService = new AuthService(apiClient, tokenStore);

        var result = await authService.RefreshTokenAsync(new RefreshDto("old-refresh"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("new-access-555", result.Data.AccessToken);
        Assert.Equal("new-access-555", await tokenStore.GetAccessTokenAsync());
        Assert.Equal("new-refresh-999", await tokenStore.GetRefreshTokenAsync());
    }

    [Fact]
    public async Task AuthService_ConfirmEmailAsync_CallsKiota_WithQueryParameters()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                var uri = req.RequestUri!.ToString();
                Assert.Contains("/api/v1/auth/confirmEmail", uri);
                Assert.Contains("userId=usr-789", uri);
                Assert.Contains("code=tok-456", uri);
                Assert.Contains("changedEmail=new%40horizon.org", uri);

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "value": true,
                            "success": true
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

        var result = await authService.ConfirmEmailAsync("usr-789", "tok-456", "new@horizon.org");

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_ResendConfirmationEmailAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/resendConfirmationEmail", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "succeeded": true,
                            "errors": []
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

        var result = await authService.ResendConfirmationEmailAsync(new EmailDto("user@horizon.org"));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.True(result.Data.Succeeded);
    }

    [Fact]
    public async Task AuthService_ForgotPasswordAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/forgotPassword", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK);
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

        var result = await authService.ForgotPasswordAsync(new EmailDto("user@horizon.org"));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AuthService_ResetPasswordAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/resetPassword", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK);
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

        var result = await authService.ResetPasswordAsync(new ResetPasswordDto("user@horizon.org", "RESET123", "NewSecurePass1!"));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AuthService_GetIdentityInfoAsync_CallsKiota_AndMapsResponse()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("/api/v1/auth/manage/info", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "email": "director@horizon.org",
                            "isEmailConfirmed": true,
                            "displayName": "Director Horizon",
                            "firstName": "Director",
                            "surName": "Horizon"
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

        var result = await authService.GetIdentityInfoAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("director@horizon.org", result.Data.Email);
        Assert.True(result.Data.IsEmailConfirmed);
        Assert.Equal("Director Horizon", result.Data.DisplayName);
    }

    [Fact]
    public async Task AuthService_UpdateIdentityInfoAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/manage/info", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK);
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

        var result = await authService.UpdateIdentityInfoAsync(new InfoDto("new@horizon.org", "NewPass123!", "OldPass123!"));

        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task AuthService_ConfigureTwoFactorAsync_CallsKiota_AndMapsResponse()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/manage/2fa", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "sharedKey": "SECRET2FAKEY",
                            "recoveryCodesLeft": 8,
                            "recoveryCodes": ["REC1", "REC2"],
                            "isTwoFactorEnabled": true,
                            "isMachineRemembered": false
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

        var result = await authService.ConfigureTwoFactorAsync(new TwoFactorDto(true, "123456", false, false, false));

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("SECRET2FAKEY", result.Data.SharedKey);
        Assert.True(result.Data.IsTwoFactorEnabled);
        Assert.Equal(8, result.Data.RecoveryCodesLeft);
    }

    [Fact]
    public async Task AuthService_ListCatalogRolesAsync_CallsKiota_AndHandles403Forbidden()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("/api/v1/auth/roles", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.Forbidden);
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

        var result = await authService.ListCatalogRolesAsync();

        Assert.False(result.Succeeded);
        Assert.Equal(403, result.StatusCode);
        Assert.Contains("Acesso negado", result.Message);
    }

    [Fact]
    public async Task AuthService_GetUserRolesAsync_CallsKiota_AndHandles404NotFound()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("/api/v1/auth/users/unknown-user/roles", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.NotFound)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": false,
                            "message": "User not found"
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

        var result = await authService.GetUserRolesAsync("unknown-user");

        Assert.False(result.Succeeded);
        Assert.Equal(404, result.StatusCode);
    }

    [Fact]
    public async Task AuthService_AssignUserRoleAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/roles", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Role assigned"
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

        var result = await authService.AssignUserRoleAsync("user-1", new AssignRoleDto("Teacher"));

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_RemoveUserRoleAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Delete, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/roles/Teacher", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Role removed"
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

        var result = await authService.RemoveUserRoleAsync("user-1", "Teacher");

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_ListCatalogPermissionsAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("/api/v1/auth/permissions", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": {
                                "permissions": ["Students.Read", "Students.Write"],
                                "groups": {
                                    "AcademicStaff": ["Students.Read"]
                                }
                            }
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

        var result = await authService.ListCatalogPermissionsAsync();

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Contains("Students.Read", result.Data.Permissions);
        Assert.True(result.Data.Groups.ContainsKey("AcademicStaff"));
    }

    [Fact]
    public async Task AuthService_GetUserPermissionsAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Get, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/permissions", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": {
                                "userId": "user-1",
                                "effectivePermissions": ["Students.Read"],
                                "directGrants": ["Students.Read"],
                                "explicitRevocations": [],
                                "assignedGroups": ["AcademicStaff"]
                            }
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

        var result = await authService.GetUserPermissionsAsync("user-1");

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Data);
        Assert.Equal("user-1", result.Data.UserId);
        Assert.Contains("Students.Read", result.Data.EffectivePermissions);
    }

    [Fact]
    public async Task AuthService_GrantUserPermissionAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/permissions/grant", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Granted"
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

        var result = await authService.GrantUserPermissionAsync("user-1", new GrantPermissionDto("Students.Write"));

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_RevokeUserPermissionAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/permissions/revoke", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Revoked"
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

        var result = await authService.RevokeUserPermissionAsync("user-1", new RevokePermissionDto("Students.Write"));

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_ClearUserPermissionAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Delete, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/permissions/Students.Write", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Cleared"
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

        var result = await authService.ClearUserPermissionAsync("user-1", "Students.Write");

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_AssignUserPermissionGroupAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Post, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/groups", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Group assigned"
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

        var result = await authService.AssignUserPermissionGroupAsync("user-1", new AssignPermissionGroupDto("AcademicStaff"));

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }

    [Fact]
    public async Task AuthService_RemoveUserPermissionGroupAsync_CallsKiota()
    {
        var testHandler = new TestHandler
        {
            Handler = req =>
            {
                Assert.Equal(HttpMethod.Delete, req.Method);
                Assert.Contains("/api/v1/auth/users/user-1/groups/AcademicStaff", req.RequestUri!.ToString());

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(
                        """
                        {
                            "success": true,
                            "value": true,
                            "message": "Group removed"
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

        var result = await authService.RemoveUserPermissionGroupAsync("user-1", "AcademicStaff");

        Assert.True(result.Succeeded);
        Assert.True(result.Data);
    }
}

