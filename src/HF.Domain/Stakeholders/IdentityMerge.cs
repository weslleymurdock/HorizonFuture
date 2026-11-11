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
    public string? ApprovedBy { get; private set; }
    public string? CompletedBy { get; private set; }
    public DateTime? RejectedAtUtc { get; private set; }
    public string? RejectedBy { get; private set; }

    public void Approve(DateTime approvedAtUtc, string approvedBy)
    {
        if (Status != IdentityMergeStatus.Proposed || approvedAtUtc.Kind != DateTimeKind.Utc)
        {
            throw new DomainException("identity_merge.invalid_approval", "Only a proposed merge can be approved with a UTC timestamp.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(approvedBy);
        Status = IdentityMergeStatus.Approved;
        ApprovedAtUtc = approvedAtUtc;
        ApprovedBy = approvedBy.Trim();
        UpdatedAt = approvedAtUtc;
    }

    public void Complete(DateTime completedAtUtc, string completedBy)
    {
        if (Status != IdentityMergeStatus.Approved || completedAtUtc.Kind != DateTimeKind.Utc || completedAtUtc < ApprovedAtUtc)
        {
            throw new DomainException("identity_merge.invalid_completion", "Only an approved merge can be completed with a valid UTC timestamp.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(completedBy);
        Status = IdentityMergeStatus.Completed;
        CompletedAtUtc = completedAtUtc;
        CompletedBy = completedBy.Trim();
        UpdatedAt = completedAtUtc;
    }

    public void Reject(string rejectedBy)
    {
        if (Status == IdentityMergeStatus.Completed)
        {
            throw new DomainException("identity_merge.completed", "A completed merge cannot be rejected.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(rejectedBy);
        Status = IdentityMergeStatus.Rejected;
        RejectedAtUtc = DateTime.UtcNow;
        RejectedBy = rejectedBy.Trim();
        UpdatedAt = DateTime.UtcNow;
    }

    private static string Require(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        return value.Trim();
    }
}
