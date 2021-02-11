using NuGet.Versioning;

namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Publishes the version
    /// </summary>
    public interface IVersionPublisher
    {
        /// <summary>
        ///     Publishes the version so that build servers can access it.
        /// </summary>
        /// <param name="version">The version to publish</param>
        void Publish(NuGetVersion version);
    }
}