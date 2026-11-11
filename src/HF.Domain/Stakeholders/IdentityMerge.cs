using HF.Domain.Common.Aggregates;
using HF.Domain.Common.Exceptions;

namespace HF.Domain.Stakeholders;

public enum IdentityMergeStatus
{
    Proposed = 1,
    Approved = 2,
    Completed = 3,
    Rejected = 4
}

public sealed class IdentityMerge : AggregateRoot
{
    private IdentityMerge() { }

    public IdentityMerge(string sourceIdentityId, string targetIdentityId, IdentityKind identityKind, string reason, string? id = null) : base(id)
    {
        if (sourceIdentityId == targetIdentityId)
        {
            throw new DomainException("identity_merge.same_identity", "Source and target identities must be different.");
        }

        SourceIdentityId = Require(sourceIdentityId);
        TargetIdentityId = Require(targetIdentityId);
        IdentityKind = identityKind;
        Reason = Require(reason);
        Status = IdentityMergeStatus.Proposed;
    }

    public string SourceIdentityId { get; private set; } = string.Empty;
    public string TargetIdentityId { get; private set; } = string.Empty;
    public IdentityKind IdentityKind { get; private set; }
    public string Reason { get; private set; } = string.Empty;
    public IdentityMergeStatus Status { get; private set; }
    public DateTime? ApprovedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }

    public void Approve(DateTime approvedAtUtc)
    {
        if (Status != IdentityMergeStatus.Proposed || approvedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("identity_merge.invalid_approval", "Only a proposed merge can be approved with a UTC timestamp.");
        }

        Status = IdentityMergeStatus.Approved;
        ApprovedAtUtc = approvedAtUtc;
        UpdatedAt = approvedAtUtc;
    }

    public void Complete(DateTime completedAtUtc)
    {
        if (Status != IdentityMergeStatus.Approved || completedAtUtc.Kind != DateTimeKind.Utc || completedAtUtc < ApprovedAtUtc)
        {
            throw new DomainException("identity_merge.invalid_completion", "Only an approved merge can be completed with a valid UTC timestamp.");
        }

        Status = IdentityMergeStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        UpdatedAt = completedAtUtc;
    }

    public void Reject()
    {
        if (Status == IdentityMergeStatus.Completed)
        {
            throw new DomainException("identity_merge.completed", "A completed merge cannot be rejected.");
        }

        Status = IdentityMergeStatus.Rejected;
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}
