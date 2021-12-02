namespace FunFair.BuildVersion.Interfaces;

/// <summary>
///     Branch settings.
/// </summary>
public interface IBranchSettings
{
    /// <summary>
    ///     The release suffix.
    /// </summary>
    string? ReleaseSuffix { get; }

    /// <summary>
    ///     The package being released.
    /// </summary>
    string? Package { get; }
}