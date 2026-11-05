using Xunit;

namespace HF.UITests.Mobile;

/// <summary>
/// Ensures all mobile Appium UI tests execute sequentially against the single Android device session.
/// </summary>
[CollectionDefinition("MobileUI", DisableParallelization = true)]
public class MobileTestCollection
{
}

