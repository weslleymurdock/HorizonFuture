using HF.Domain.Common.Exceptions;
using HF.Domain.Common.ValueObjects;

namespace HF.Domain.SocialServices.Activities;

public sealed class SessionAttendanceRecord : ValueObject
{
    public SessionAttendanceRecord(
        string beneficiaryPersonId,
        AttendanceStatus status,
        string recordedByPersonId,
        string? justification = null,
        DateTime? recordedAtUtc = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(beneficiaryPersonId);
        ArgumentException.ThrowIfNullOrWhiteSpace(recordedByPersonId);

        if (status == AttendanceStatus.JustifiedAbsence && string.IsNullOrWhiteSpace(justification))
        {
            throw new DomainException("attendance.justification_required", "A justification is required for a justified absence.");
        }

        BeneficiaryPersonId = beneficiaryPersonId.Trim();
        Status = status;
        RecordedByPersonId = recordedByPersonId.Trim();
        Justification = string.IsNullOrWhiteSpace(justification) ? null : justification.Trim();
        RecordedAtUtc = recordedAtUtc ?? DateTime.UtcNow;
    }

    public string BeneficiaryPersonId { get; }
    public AttendanceStatus Status { get; }
    public string RecordedByPersonId { get; }
    public string? Justification { get; }
    public DateTime RecordedAtUtc { get; }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return BeneficiaryPersonId;
        yield return Status;
        yield return RecordedByPersonId;
        yield return Justification;
    }
}
