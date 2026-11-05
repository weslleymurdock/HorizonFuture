using System;
using System.Threading.Tasks;
using HF.Shared.PageModels.Auth;
using HF.UITests.Shared.Fakes;
using Xunit;

namespace HF.UITests.Shared;

public class SharedAuthPageModelsTests
{
    private readonly FakeAuthService _fakeAuthService = new();

    [Fact]
    public async Task LoginPageModel_EmptyInputs_SetsErrorMessageAndDoesNotCallApi()
    {
        var model = new LoginPageModel(_fakeAuthService);

        var result = await model.LoginAsync();

        Assert.False(result);
        Assert.False(model.IsLoggedIn);
        Assert.NotNull(model.ErrorMessage);
        Assert.Contains("email é obrigatório", model.ErrorMessage);
    }

    [Fact]
    public async Task LoginPageModel_ValidCredentials_LogsInSuccessfully()
    {
        var model = new LoginPageModel(_fakeAuthService)
        {
            Email = "admin@horizon.org",
            Password = "Password123!"
        };

        var result = await model.LoginAsync();

        Assert.True(result);
        Assert.True(model.IsLoggedIn);
        Assert.Null(model.ErrorMessage);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task LoginPageModel_ApiFailure_SetsErrorMessage()
    {
        _fakeAuthService.ShouldSucceed = false;
        _fakeAuthService.FailureMessage = "Credenciais inválidas";

        var model = new LoginPageModel(_fakeAuthService)
        {
            Email = "wrong@horizon.org",
            Password = "BadPassword"
        };

        var result = await model.LoginAsync();

        Assert.False(result);
        Assert.False(model.IsLoggedIn);
        Assert.Equal("Credenciais inválidas", model.ErrorMessage);
    }

    [Fact]
    public async Task RegisterPageModel_MismatchedPasswords_FailsValidation()
    {
        var model = new RegisterPageModel(_fakeAuthService)
        {
            Email = "newuser@horizon.org",
            Password = "Password123!",
            ConfirmPassword = "DifferentPassword123!"
        };

        var result = await model.RegisterAsync();

        Assert.False(result);
        Assert.False(model.IsRegistered);
        Assert.Equal("As senhas não coincidem.", model.ErrorMessage);
    }

    [Fact]
    public async Task RegisterPageModel_ValidInputs_RegistersSuccessfully()
    {
        var model = new RegisterPageModel(_fakeAuthService)
        {
            Email = "newuser@horizon.org",
            Password = "Password123!",
            ConfirmPassword = "Password123!"
        };

        var result = await model.RegisterAsync();

        Assert.True(result);
        Assert.True(model.IsRegistered);
        Assert.Null(model.ErrorMessage);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ForgotPasswordPageModel_SendsRecoveryInstructions()
    {
        var model = new ForgotPasswordPageModel(_fakeAuthService)
        {
            Email = "user@horizon.org"
        };

        var result = await model.SendForgotPasswordAsync();

        Assert.True(result);
        Assert.True(model.EmailSent);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ResetPasswordPageModel_ValidCodeAndMatchingPasswords_ResetsSuccessfully()
    {
        var model = new ResetPasswordPageModel(_fakeAuthService)
        {
            Email = "user@horizon.org",
            ResetCode = "CODE123",
            NewPassword = "NewPassword123!",
            ConfirmNewPassword = "NewPassword123!"
        };

        var result = await model.ResetPasswordAsync();

        Assert.True(result);
        Assert.True(model.IsPasswordReset);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ConfirmEmailPageModel_ValidInputs_ConfirmsSuccessfully()
    {
        var model = new ConfirmEmailPageModel(_fakeAuthService)
        {
            UserId = "user-123",
            Code = "CONFIRM-CODE"
        };

        var result = await model.ConfirmEmailAsync();

        Assert.True(result);
        Assert.True(model.IsConfirmed);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ConfirmEmailPageModel_ResendEmail_TriggersApi()
    {
        var model = new ConfirmEmailPageModel(_fakeAuthService)
        {
            ResendEmail = "user@horizon.org"
        };

        var result = await model.ResendConfirmationEmailAsync();

        Assert.True(result);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ManageProfilePageModel_LoadsInfoAndUpdates()
    {
        var model = new ManageProfilePageModel(_fakeAuthService);

        var loadResult = await model.LoadInfoAsync();
        Assert.True(loadResult);
        Assert.Equal("user@horizon.org", model.Email);
        Assert.True(model.IsEmailConfirmed);

        // Update info without old password should fail
        var failUpdate = await model.UpdateInfoAsync();
        Assert.False(failUpdate);
        Assert.Contains("senha atual", model.ErrorMessage);

        // Update with old password should succeed
        model.OldPassword = "OldPassword123!";
        model.NewEmail = "newemail@horizon.org";
        var successUpdate = await model.UpdateInfoAsync();
        Assert.True(successUpdate);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ManageProfilePageModel_TwoFactorAndTokens_ExecuteCorrectly()
    {
        var model = new ManageProfilePageModel(_fakeAuthService);

        // Configure 2FA
        var twoFaResult = await model.ConfigureTwoFactorAsync(enable: true);
        Assert.True(twoFaResult);
        Assert.True(model.IsTwoFactorEnabled);
        Assert.NotNull(model.SharedKey);

        // Refresh token
        model.RefreshTokenInput = "valid-refresh-token";
        var refreshResult = await model.RefreshTokensAsync();
        Assert.True(refreshResult);
        Assert.NotNull(model.SuccessMessage);

        // Revoke token
        var revokeResult = await model.RevokeTokenAsync();
        Assert.True(revokeResult);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ManageRolesPageModel_LoadsCatalogAndManagesUserRoles()
    {
        var model = new ManageRolesPageModel(_fakeAuthService)
        {
            TargetUserId = "user-role-test"
        };

        var loadCatalog = await model.LoadCatalogRolesAsync();
        Assert.True(loadCatalog);
        Assert.NotEmpty(model.CatalogRoles);

        var loadUserRoles = await model.LoadUserRolesAsync();
        Assert.True(loadUserRoles);
        Assert.Contains("Teacher", model.UserRoles);

        // Assign role
        model.SelectedRoleToAssign = "Manager";
        var assignResult = await model.AssignRoleAsync();
        Assert.True(assignResult);

        // Remove role
        var removeResult = await model.RemoveRoleAsync("Teacher");
        Assert.True(removeResult);
    }

    [Fact]
    public async Task ManagePermissionsPageModel_LoadsCatalogAndManagesGrantsAndGroups()
    {
        var model = new ManagePermissionsPageModel(_fakeAuthService)
        {
            TargetUserId = "user-perm-test"
        };

        var loadCatalog = await model.LoadCatalogPermissionsAsync();
        Assert.True(loadCatalog);
        Assert.NotEmpty(model.CatalogPermissions);
        Assert.NotEmpty(model.CatalogGroups);

        var loadUserPerms = await model.LoadUserPermissionsAsync();
        Assert.True(loadUserPerms);

        // Grant permission
        model.SelectedPermissionToGrant = "Students.Write";
        var grantResult = await model.GrantPermissionAsync();
        Assert.True(grantResult);

        // Revoke permission
        model.SelectedPermissionToRevoke = "Students.Read";
        var revokeResult = await model.RevokePermissionAsync();
        Assert.True(revokeResult);

        // Clear permission override
        var clearResult = await model.ClearPermissionAsync("Students.Read");
        Assert.True(clearResult);

        // Assign group
        model.SelectedGroupToAssign = "AcademicStaff";
        var assignGroup = await model.AssignGroupAsync();
        Assert.True(assignGroup);

        // Remove group
        var removeGroup = await model.RemoveGroupAsync("AcademicStaff");
        Assert.True(removeGroup);
    }

    [Fact]
    public async Task ManageProfilePageModel_RefreshToken_PreFillsFromTokenStore_AndUsesIt()
    {
        var tokenStore = new HF.Shared.Auth.InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("current-access", "stored-refresh-xyz");

        var model = new ManageProfilePageModel(_fakeAuthService, tokenStore);

        await model.LoadInfoAsync();
        Assert.Equal("stored-refresh-xyz", model.RefreshTokenInput);

        // When refreshing without typing, it uses the stored refresh token
        model.RefreshTokenInput = string.Empty;
        var refreshResult = await model.RefreshTokensAsync();
        Assert.True(refreshResult);
        Assert.NotNull(model.SuccessMessage);
    }

    [Fact]
    public async Task ManageProfilePageModel_RevokeToken_ClearsProfileState()
    {
        var tokenStore = new HF.Shared.Auth.InMemoryTokenStore();
        await tokenStore.SaveTokensAsync("current-access", "stored-refresh-xyz");

        var model = new ManageProfilePageModel(_fakeAuthService, tokenStore)
        {
            Email = "user@horizon.org",
            DisplayName = "Test User",
            RefreshTokenInput = "token-to-clear"
        };

        var revokeResult = await model.RevokeTokenAsync();
        Assert.True(revokeResult);
        Assert.Empty(model.Email);
        Assert.Null(model.DisplayName);
        Assert.Null(model.RefreshTokenInput);
    }

    [Fact]
    public async Task ManageProfilePageModel_RefreshTokens_EmptyInputAndNoStore_Fails()
    {
        var model = new ManageProfilePageModel(_fakeAuthService);

        var refreshResult = await model.RefreshTokensAsync();
        Assert.False(refreshResult);
        Assert.Contains("refresh token", model.ErrorMessage);
    }

    [Fact]
    public async Task ManageRolesPageModel_ValidationAndErrors_SetErrorMessage()
    {
        var model = new ManageRolesPageModel(_fakeAuthService);

        // Missing user ID
        var loadResult = await model.LoadUserRolesAsync();
        Assert.False(loadResult);
        Assert.Contains("ID do usuário", model.ErrorMessage);

        var assignMissingUser = await model.AssignRoleAsync();
        Assert.False(assignMissingUser);

        model.TargetUserId = "user-123";
        var assignMissingRole = await model.AssignRoleAsync();
        Assert.False(assignMissingRole);
        Assert.Contains("Selecione a role", model.ErrorMessage);

        // Simulated API Error / Unauthorized
        _fakeAuthService.ShouldSucceed = false;
        _fakeAuthService.FailureMessage = "Acesso negado: permissão Roles.Manage necessária (403)";
        model.SelectedRoleToAssign = "Director";

        var failedAssign = await model.AssignRoleAsync();
        Assert.False(failedAssign);
        Assert.Equal("Acesso negado: permissão Roles.Manage necessária (403)", model.ErrorMessage);
    }

    [Fact]
    public async Task ManagePermissionsPageModel_ValidationAndErrors_SetErrorMessage()
    {
        var model = new ManagePermissionsPageModel(_fakeAuthService);

        // Missing user ID
        var loadResult = await model.LoadUserPermissionsAsync();
        Assert.False(loadResult);
        Assert.Contains("ID do usuário", model.ErrorMessage);

        model.TargetUserId = "user-123";

        // Missing grant selection
        var grantMissing = await model.GrantPermissionAsync();
        Assert.False(grantMissing);
        Assert.Contains("Selecione a permissão", model.ErrorMessage);

        // Missing revoke selection
        var revokeMissing = await model.RevokePermissionAsync();
        Assert.False(revokeMissing);
        Assert.Contains("Selecione a permissão", model.ErrorMessage);

        // Missing group selection
        var groupMissing = await model.AssignGroupAsync();
        Assert.False(groupMissing);
        Assert.Contains("Selecione o grupo", model.ErrorMessage);

        // Simulated API Error / Unauthorized
        _fakeAuthService.ShouldSucceed = false;
        _fakeAuthService.FailureMessage = "Acesso negado: permissão Permissions.Manage necessária (403)";
        model.SelectedPermissionToGrant = "Students.Write";

        var failedGrant = await model.GrantPermissionAsync();
        Assert.False(failedGrant);
        Assert.Equal("Acesso negado: permissão Permissions.Manage necessária (403)", model.ErrorMessage);
    }

    [Fact]
    public async Task ConfirmEmailPageModel_MissingInputs_FailsValidation()
    {
        var model = new ConfirmEmailPageModel(_fakeAuthService);

        var resultNoUserId = await model.ConfirmEmailAsync();
        Assert.False(resultNoUserId);
        Assert.Contains("identificador", model.ErrorMessage);

        model.UserId = "user-123";
        var resultNoCode = await model.ConfirmEmailAsync();
        Assert.False(resultNoCode);
        Assert.Contains("código", model.ErrorMessage);

        var resendNoEmail = await model.ResendConfirmationEmailAsync();
        Assert.False(resendNoEmail);
        Assert.Contains("email", model.ErrorMessage);
    }

    [Fact]
    public async Task ResetPasswordPageModel_Validation_FailsOnEmptyOrMismatch()
    {
        var model = new ResetPasswordPageModel(_fakeAuthService);

        var resultEmpty = await model.ResetPasswordAsync();
        Assert.False(resultEmpty);
        Assert.Contains("email", model.ErrorMessage);

        model.Email = "user@horizon.org";
        var resultNoCode = await model.ResetPasswordAsync();
        Assert.False(resultNoCode);
        Assert.Contains("código", model.ErrorMessage);

        model.ResetCode = "123456";
        var resultNoPassword = await model.ResetPasswordAsync();
        Assert.False(resultNoPassword);
        Assert.Contains("nova senha", model.ErrorMessage);

        model.NewPassword = "Pass1";
        model.ConfirmNewPassword = "Pass2";
        var resultMismatch = await model.ResetPasswordAsync();
        Assert.False(resultMismatch);
        Assert.Equal("As senhas não coincidem.", model.ErrorMessage);
    }

    [Fact]
    public async Task ForgotPasswordPageModel_EmptyEmail_FailsValidation()
    {
        var model = new ForgotPasswordPageModel(_fakeAuthService);

        var result = await model.SendForgotPasswordAsync();
        Assert.False(result);
        Assert.Contains("email", model.ErrorMessage);
    }
}

