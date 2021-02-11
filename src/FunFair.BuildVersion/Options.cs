using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace FunFair.BuildVersion
{
    /// <summary>
    ///     Options
    /// </summary>
    [DebuggerDisplay("Build {BuildNumber}, Release: {ReleaseSuffix} Package: {Package}")]
    [SuppressMessage(category: "ReSharper", checkId: "ClassNeverInstantiated.Global", Justification = "Created internally by command line parser")]
    public sealed record Options
    {
        /// <summary>
        ///     Whether warnings should be errors.
        /// </summary>
        [Option(shortName: 'x', longName: "WarningsAsErrors", Required = false, HelpText = "Whether warnings should be errors", Default = false)]
        [SuppressMessage(category: "ReSharper", checkId: "UnusedAutoPropertyAccessor.Global", Justification = "Used by command line parser")]
        public bool WarningsAsErrors { get; init; }

        /// <summary>
        ///     The build number, if knowable at the command line
        /// </summary>
        [Option(shortName: 'b', longName: "BuildNumber", Required = false, HelpText = "The build number", Default = -1)]
        public int BuildNumber { get; init; }

        /// <summary>
        ///     The Release Suffix e.g. "games" becomes "release-games" when looking for releases
        /// </summary>
        [Option(shortName: 's', longName: "ReleaseSuffix", Required = false, HelpText = "The release suffix", Default = "")]
        public string? ReleaseSuffix { get; init; }

        /// <summary>
        ///     The package being released.
        /// </summary>
        [Option(shortName: 'p', longName: "Package", Required = false, HelpText = "The package being released", Default = "")]
        public string? Package { get; init; }
    }
}