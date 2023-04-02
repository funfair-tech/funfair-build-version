using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces;

public interface IVersionPublisher
{
    void Publish(NuGetVersion version);
}