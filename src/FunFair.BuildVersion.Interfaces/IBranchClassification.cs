using System.Diagnostics.CodeAnalysis;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces;

public interface IBranchClassification
{
    bool IsRelease(string branchName, [NotNullWhen(true)] out NuGetVersion? version);

    bool IsPullRequest(string currentBranch, out long pullRequestId);
}