using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces;

/// <summary>
///     Version Detection.
/// </summary>
public interface IVersionDetector
{
    /// <summary>
    ///     Finds the version for the current branch.
    /// </summary>
    /// <param name="buildNumber">The current build number.</param>
    /// <returns>The version number.</returns>
    NuGetVersion FindVersion(int buildNumber);
}