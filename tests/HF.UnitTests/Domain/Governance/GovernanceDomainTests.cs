using HF.Domain.Common.Exceptions;
using HF.Domain.Governance;
using HF.Domain.Stakeholders;
using Xunit;

namespace HF.UnitTests.Domain.Governance;

public sealed class GovernanceDomainTests
{
    [Fact]
    public void Mandate_TracksEvidenceAndEndDate()
    {
        var mandate = new GovernanceMandate(
            "org-1",
            "body-1",
            "person-1",
            "Presidente",
            new DateOnly(2026, 1, 1));

        mandate.SetEvidenceReference("ata-2026-01");
        mandate.End(new DateOnly(2026, 12, 31));

        Assert.Equal(GovernanceMandateStatus.Ended, mandate.Status);
        Assert.Equal("ata-2026-01", mandate.EvidenceReference);
    }

    [Fact]
    public void IdentityMerge_RequiresApprovalBeforeCompletion()
    {
        var merge = new IdentityMerge("source", "target", IdentityKind.Person, "Confirmed duplicate");
        var completion = new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);

        Assert.Throws<DomainException>((Action)(() => merge.Complete(completion, "reviewer")));

        merge.Approve(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "reviewer");
        merge.Complete(completion, "operator");

        Assert.Equal(IdentityMergeStatus.Completed, merge.Status);
    }

    [Fact]
    public void IdentityMatching_DoesNotMergeByNameAlone()
    {
        var left = new Person("Ana Silva");
        var right = new Person("Ana Silva");

        var result = IdentityMatching.Compare(left, right);

        Assert.Equal(DuplicateMatchKind.None, result.Kind);
    }
}
