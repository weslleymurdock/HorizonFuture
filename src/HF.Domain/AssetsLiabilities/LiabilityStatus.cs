namespace HF.Domain.AssetsLiabilities;

/// <summary>
/// Settlement status of a financial obligation.
/// </summary>
public enum LiabilityStatus
{
    Pending = 1,
    Paid = 2,
    Overdue = 3,
    Cancelled = 4,
    Renegotiated = 5
}

