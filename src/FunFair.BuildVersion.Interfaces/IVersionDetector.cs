using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces;

public interface IVersionDetector
{
    NuGetVersion FindVersion(int buildNumber);
}