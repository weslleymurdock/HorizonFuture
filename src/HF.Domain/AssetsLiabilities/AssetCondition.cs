namespace HF.Domain.AssetsLiabilities;

/// <summary>
/// Physical condition of a patrimonial asset.
/// </summary>
public enum AssetCondition
{
    New = 1,
    Good = 2,
    Fair = 3,
    NeedsRepair = 4,
    Damaged = 5,
    Obsolete = 6
}

