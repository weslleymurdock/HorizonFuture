namespace HF.Domain.AssetsLiabilities;

/// <summary>
/// Operational status of an asset.
/// </summary>
public enum AssetStatus
{
    InUse = 1,
    InStorage = 2,
    UnderMaintenance = 3,
    Disposed = 4,
    Donated = 5
}

