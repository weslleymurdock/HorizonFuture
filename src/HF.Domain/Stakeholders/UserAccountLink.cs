using HF.Domain.Common.Entities;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public sealed class UserAccountLink : EntityBase
{
    private UserAccountLink() { }

    public UserAccountLink(string identityId, IdentityKind identityKind, string accountId, DateTime linkedAtUtc, string? id = null) : base(id)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(identityId);
        ArgumentException.ThrowIfNullOrWhiteSpace(accountId);
        if (linkedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("account_link.utc_required", "Account link timestamps must be UTC.");
        }

        IdentityId = identityId;
        IdentityKind = identityKind;
        AccountId = accountId;
        LinkedAtUtc = linkedAtUtc;
        IsActive = true;
    }

    public string IdentityId { get; private set; } = string.Empty;
    public IdentityKind IdentityKind { get; private set; }
    public string AccountId { get; private set; } = string.Empty;
    public DateTime LinkedAtUtc { get; private set; }
    public DateTime? UnlinkedAtUtc { get; private set; }
    public bool IsActive { get; private set; }

    public void Unlink(DateTime unlinkedAtUtc)
    {
        if (!IsActive)
        {
            throw new DomainException("account_link.already_unlinked", "The account link is already inactive.");
        }

        if (unlinkedAtUtc.Kind != DateTimeKind.Utc || unlinkedAtUtc < LinkedAtUtc)
        {
            throw new DomainException("account_link.invalid_end", "The unlink timestamp must be UTC and cannot precede linking.");
        }

        IsActive = false;
        UnlinkedAtUtc = unlinkedAtUtc;
        UpdatedAt = unlinkedAtUtc;
    }
}
