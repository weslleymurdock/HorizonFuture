using HF.Application.Auth.Abstract;
using HF.Application.Auth.Responses;

namespace HF.Application.Tests.Common;

public class FakeIdentityService : IIdentityService
{
    private readonly HashSet<string> _existingUserIds = [];

    public void AddUser(string userId) => _existingUserIds.Add(userId);

    public Task<IdentityInfoResponse?> GetInfoAsync(string userId, CancellationToken cancellationToken)
    {
        if (_existingUserIds.Contains(userId))
        {
            return Task.FromResult<IdentityInfoResponse?>(new IdentityInfoResponse($"{userId}@example.com", true, userId));
        }

        return Task.FromResult<IdentityInfoResponse?>(null);
    }

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) => Task.FromResult(false);

    public Task<IdentityResultResponse> RegisterAsync(string email, string password, CancellationToken cancellationToken) =>
        Task.FromResult(new IdentityResultResponse(true, []));

    public Task<TokenResponse?> LoginAsync(string email, string password, string? twoFactorCode, string? twoFactorRecoveryCode, CancellationToken cancellationToken) =>
        Task.FromResult<TokenResponse?>(null);

    public Task<TokenResponse?> RefreshAsync(string refreshToken, CancellationToken cancellationToken) =>
        Task.FromResult<TokenResponse?>(null);

    public Task<bool> RevokeAsync(string accessToken, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task<bool> ConfirmEmailAsync(string userId, string code, string? changedEmail, CancellationToken cancellationToken) =>
        Task.FromResult(true);

    public Task<IdentityResultResponse> ResendConfirmationEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(new IdentityResultResponse(true, []));

    public Task<IdentityResultResponse> ForgotPasswordAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(new IdentityResultResponse(true, []));

    public Task<IdentityResultResponse> ResetPasswordAsync(string email, string resetCode, string newPassword, CancellationToken cancellationToken) =>
        Task.FromResult(new IdentityResultResponse(true, []));

    public Task<IdentityResultResponse> UpdateInfoAsync(string userId, string? newEmail, string? newPassword, string oldPassword, CancellationToken cancellationToken) =>
        Task.FromResult(new IdentityResultResponse(true, []));

    public Task<TwoFactorResponse?> ConfigureTwoFactorAsync(string userId, bool? enable, string? twoFactorCode, bool resetRecoveryCodes, bool resetSharedKey, bool forgetMachine, CancellationToken cancellationToken) =>
        Task.FromResult<TwoFactorResponse?>(null);
}
