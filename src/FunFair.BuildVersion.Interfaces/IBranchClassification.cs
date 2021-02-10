using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Branch Classification
    /// </summary>
    public interface IBranchClassification
    {
        /// <summary>
        ///     Checks to see if the branch is a release branch.
        /// </summary>
        /// <param name="branchName">The branch to check</param>
        /// <param name="version">The version that was found, if the branch is a release branch.</param>
        /// <returns>true, if the branch is a release branch; otherwise, false.</returns>
        bool IsReleaseBranch(string branchName, [NotNullWhen(true)] out NuGetVersion? version);
    }
}