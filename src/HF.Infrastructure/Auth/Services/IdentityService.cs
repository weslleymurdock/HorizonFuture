using HF.Application.Auth.Responses;
using HF.Application.Auth.Abstract;
using HF.Infrastructure.Auth.Models;
using HF.Infrastructure.Auth.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.Extensions.Logging;

namespace HF.Infrastructure.Auth.Services;

/// <summary>
/// Provides identity operations for users, authentication, account recovery, and two-factor authentication.
/// </summary>
/// <param name="userManager">The user manager used to manage application users.</param>
/// <param name="roleManager">The role manager used to manage application roles.</param>
/// <param name="signInManager">The sign-in manager used to validate user passwords and lockout state.</param>
/// <param name="tokenService">The service used to create and validate JWT tokens.</param>
/// <param name="revokedTokenStore">The store used to track revoked tokens.</param>
/// <param name="emailSender">The service used to send identity-related email messages.</param>
public sealed class IdentityService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager,
    SignInManager<User> signInManager,
    IJwtTokenService tokenService,
    IRevokedTokenStore revokedTokenStore,
    IIdentityEmailSender emailSender) : IIdentityService
{
    /// <summary>
    /// Registers a new user and sends an email confirmation link.
    /// </summary>
    public async Task<IdentityResultResponse> RegisterAsync(string email, string password, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var user = new User(email)
        {
            Email = email,
            DisplayName = string.Empty,
            FirstName = string.Empty,
            SurName = string.Empty,
            EmailConfirmed = false
        };

        IdentityResult result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            return Failure(result);
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendConfirmationAsync(
            email,
            $"/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    /// <summary>
    /// Authenticates a user and issues JWT access and refresh tokens when all required authentication factors are valid.
    /// </summary>
    public async Task<TokenResponse?> LoginAsync(
        string email,
        string password,
        string? twoFactorCode,
        string? twoFactorRecoveryCode,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return null;
        }

        SignInResult passwordResult = await signInManager.CheckPasswordSignInAsync(user, password, true);
        if (passwordResult.IsLockedOut || passwordResult.IsNotAllowed || !passwordResult.Succeeded)
        {
            return null;
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            var valid = !string.IsNullOrWhiteSpace(twoFactorCode)
                ? await userManager.VerifyTwoFactorTokenAsync(
                    user,
                    userManager.Options.Tokens.AuthenticatorTokenProvider,
                    twoFactorCode)
                : !string.IsNullOrWhiteSpace(twoFactorRecoveryCode) &&
                  (await userManager.RedeemTwoFactorRecoveryCodeAsync(user, twoFactorRecoveryCode)).Succeeded;

            if (!valid)
            {
                return null;
            }
        }

        return await CreateUserTokensAsync(user);
    }
 
    /// <summary>
    /// Exchanges a valid refresh token for a new access and refresh token pair and revokes the previous refresh token.
    /// </summary>
    public async Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ClaimsPrincipal? principal = tokenService.ValidateToken(refreshToken);
        if (principal is null || !string.Equals(principal.FindFirstValue("token_type"), JwtTokenTypes.Refresh, StringComparison.Ordinal))
        {
            return null;
        }

        var userId = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (string.IsNullOrWhiteSpace(userId))
        {
            userId = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        if (string.IsNullOrWhiteSpace(userId))
        {
            return null;
        }

        User? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        var tokenId = tokenService.GetTokenId(refreshToken);
        DateTime? expiration = tokenService.GetExpiration(refreshToken);
        TokenResponse tokens = await CreateUserTokensAsync(user);

        if (!string.IsNullOrWhiteSpace(tokenId) && expiration.HasValue)
        {
            revokedTokenStore.Revoke(tokenId, expiration.Value);
        }

        return tokens;
    }

    /// <summary>
    /// Revokes a valid access token until its natural expiration.
    /// </summary>
    public Task<bool> RevokeAsync(string accessToken, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ClaimsPrincipal? principal = tokenService.ValidateToken(accessToken);
        var tokenId = tokenService.GetTokenId(accessToken);
        DateTime? expiration = tokenService.GetExpiration(accessToken);

        if (principal is null || string.IsNullOrWhiteSpace(tokenId) || !expiration.HasValue)
        {
            return Task.FromResult(false);
        }

        revokedTokenStore.Revoke(tokenId, expiration.Value);
        return Task.FromResult(true);
    }

    /// <summary>
    /// Confirms a user's email address or confirms a changed email address using a supplied Identity token.
    /// </summary>
    public async Task<bool> ConfirmEmailAsync(string userId, string code, string? changedEmail, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return false;
        }

        IdentityResult result = !string.IsNullOrWhiteSpace(changedEmail)
            ? await userManager.ChangeEmailAsync(user, changedEmail, code)
            : await userManager.ConfirmEmailAsync(user, code);

        return result.Succeeded;
    }

    /// <summary>
    /// Resends an email confirmation link when the specified user exists and remains unconfirmed.
    /// </summary>
    public async Task<IdentityResultResponse> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null || await userManager.IsEmailConfirmedAsync(user))
        {
            return IdentityResultResponse.Success();
        }

        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        await emailSender.SendConfirmationAsync(
            email,
            $"/confirmEmail?userId={Uri.EscapeDataString(user.Id)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    /// <summary>
    /// Starts a password recovery operation for a user with a password.
    /// </summary>
    public async Task<IdentityResultResponse> ForgotPasswordAsync(string email, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null || !await userManager.HasPasswordAsync(user))
        {
            return IdentityResultResponse.Success();
        }

        var code = await userManager.GeneratePasswordResetTokenAsync(user);
        await emailSender.SendPasswordResetAsync(
            email,
            $"/resetPassword?email={Uri.EscapeDataString(email)}&code={Uri.EscapeDataString(code)}",
            cancellationToken);

        return IdentityResultResponse.Success();
    }

    /// <summary>
    /// Resets a user's password using a valid password-reset token.
    /// </summary>
    public async Task<IdentityResultResponse> ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByEmailAsync(email);
        if (user is null)
        {
            return IdentityResultResponse.Failure(["Invalid password reset request."]);
        }

        IdentityResult result = await userManager.ResetPasswordAsync(user, resetCode, newPassword);
        return result.Succeeded ? IdentityResultResponse.Success() : Failure(result);
    }

    /// <summary>
    /// Gets identity information for a user.
    /// </summary>
    public async Task<IdentityInfoResponse?> GetInfoAsync(string userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByIdAsync(userId);
        return user is null
            ? null
            : new IdentityInfoResponse(
                user.Email ?? string.Empty,
                await userManager.IsEmailConfirmedAsync(user),
                user.DisplayName,
                user.FirstName,
                user.SurName);
    }

    /// <summary>
    /// Updates a user's email address and password after validating the current password.
    /// </summary>
    public async Task<IdentityResultResponse> UpdateInfoAsync(string userId, string? newEmail, string? newPassword, string oldPassword, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByIdAsync(userId);
        if (user is null || !await userManager.CheckPasswordAsync(user, oldPassword))
        {
            return IdentityResultResponse.Failure(["The current credentials are invalid."]);
        }

        if (!string.IsNullOrWhiteSpace(newEmail) && !string.Equals(user.Email, newEmail, StringComparison.OrdinalIgnoreCase))
        {
            var emailCode = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            IdentityResult emailResult = await userManager.ChangeEmailAsync(user, newEmail, emailCode);
            if (!emailResult.Succeeded)
            {
                return Failure(emailResult);
            }
        }

        if (!string.IsNullOrWhiteSpace(newPassword))
        {
            IdentityResult passwordResult = await userManager.ChangePasswordAsync(user, oldPassword, newPassword);
            if (!passwordResult.Succeeded)
            {
                return Failure(passwordResult);
            }
        }

        return IdentityResultResponse.Success();
    }

    /// <summary>
    /// Configures authenticator-based two-factor authentication and optionally rotates its recovery material.
    /// </summary>
    public async Task<TwoFactorResponse?> ConfigureTwoFactorAsync(string userId, bool? enable, string? twoFactorCode, bool resetRecoveryCodes, bool resetSharedKey, bool forgetMachine, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        User? user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return null;
        }

        if (enable == true)
        {
            if (string.IsNullOrWhiteSpace(twoFactorCode) || !await userManager.VerifyTwoFactorTokenAsync(user, userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode))
            {
                return null;
            }

            _ = await userManager.SetTwoFactorEnabledAsync(user, true);
        }
        else if (enable == false || resetSharedKey)
        {
            _ = await userManager.SetTwoFactorEnabledAsync(user, false);
        }

        if (resetSharedKey)
        {
            _ = await userManager.ResetAuthenticatorKeyAsync(user);
        }

        string[]? recoveryCodes = null;
        if (resetRecoveryCodes || (enable == true && await userManager.CountRecoveryCodesAsync(user) == 0))
        {
            recoveryCodes = (await userManager.GenerateNewTwoFactorRecoveryCodesAsync(user, 10))?.ToArray();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            _ = await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var recoveryCodesLeft = await userManager.CountRecoveryCodesAsync(user);
        return new TwoFactorResponse(key, recoveryCodesLeft, recoveryCodes, await userManager.GetTwoFactorEnabledAsync(user), false);
    }

    private async Task<TokenResponse> CreateUserTokensAsync(User user)
    {
        IList<string> roles = await userManager.GetRolesAsync(user);
        IList<Claim> claims = await userManager.GetClaimsAsync(user);

        foreach (var roleName in roles)
        {
            Role? role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            claims = [.. claims, .. await roleManager.GetClaimsAsync(role)];
        }

        string identityName = !string.IsNullOrWhiteSpace(user.Email)
            ? user.Email
            : (!string.IsNullOrWhiteSpace(user.UserName) ? user.UserName : user.Id);

        return tokenService.CreateTokens(user.Id, identityName, roles, claims);
    }

    private static IdentityResultResponse Failure(IdentityResult result) => IdentityResultResponse.Failure(result.Errors.Select(error => error.Description));

    /// <inheritdoc/>
    public async Task<bool> EmailExistsAsync(
        string email,
        CancellationToken cancellationToken)
        => await userManager
            .Users
            .AsNoTracking()
            .AnyAsync(x =>
                x.Email == email,
                cancellationToken);
}

/// <summary>
/// Logs identity email messages instead of delivering them through an external email provider.
/// </summary>
public sealed partial class LoggingIdentityEmailSender(ILogger<LoggingIdentityEmailSender> logger) : IIdentityEmailSender
{
    public Task SendConfirmationAsync(string email, string confirmationLink, CancellationToken cancellationToken)
    {
        LogSendConfirmation(logger, email, confirmationLink);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string email, string resetLink, CancellationToken cancellationToken)
    {
        LogSendPassword(logger, email, resetLink);
        return Task.CompletedTask;
    }

    [LoggerMessage(0420, LogLevel.Information, "Identity confirmation link for {Email}: {Link}")]
    static partial void LogSendConfirmation(ILogger logger, string email, string link);

    [LoggerMessage(0240, LogLevel.Information, "Identity password reset link for {Email}: {Link}")]
    static partial void LogSendPassword(ILogger logger, string email, string link);
}
