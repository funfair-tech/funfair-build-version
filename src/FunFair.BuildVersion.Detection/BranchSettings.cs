using System.Diagnostics;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection
{
    /// <summary>
    ///     Branch settings
    /// </summary>
    [DebuggerDisplay("Suffix: {ReleaseSuffix} Package: {Package}")]
    public sealed class BranchSettings : IBranchSettings
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="releaseSuffix">Release suffix.</param>
        /// <param name="package">Package being released.</param>
        public BranchSettings(string? releaseSuffix, string? package)
        {
            this.ReleaseSuffix = releaseSuffix;
            this.Package = package;
        }

        /// <inheritdoc />
        public string? ReleaseSuffix { get; }

        /// <inheritdoc />
        public string? Package { get; }
    }
}