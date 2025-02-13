using LibGit2Sharp;
using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces;

public interface IVersionDetector
{
    NuGetVersion FindVersion(Repository repository, int buildNumber);
}
