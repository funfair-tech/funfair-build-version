using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using CommandLine;

namespace FunFair.BuildVersion;

[DebuggerDisplay("Build {BuildNumber}, Release: {ReleaseSuffix} Package: {Package}")]
[SuppressMessage(
    category: "ReSharper",
    checkId: "ClassNeverInstantiated.Global",
    Justification = "Created internally by command line parser"
)]
public sealed record Options
{
    [Option(
        shortName: 'x',
        longName: "WarningsAsErrors",
        Required = false,
        HelpText = "Whether warnings should be errors",
        Default = false
    )]
    [SuppressMessage(
        category: "ReSharper",
        checkId: "UnusedAutoPropertyAccessor.Global",
        Justification = "Used by command line parser"
    )]
    public bool WarningsAsErrors { get; init; }

    [Option(
        shortName: 'b',
        longName: "BuildNumber",
        Required = false,
        HelpText = "The build number",
        Default = -1
    )]
    public int BuildNumber { get; init; }

    [Option(
        shortName: 's',
        longName: "ReleaseSuffix",
        Required = false,
        HelpText = "The release suffix",
        Default = ""
    )]
    public string? ReleaseSuffix { get; init; }

    [Option(
        shortName: 'p',
        longName: "Package",
        Required = false,
        HelpText = "The package being released",
        Default = ""
    )]
    public string? Package { get; init; }

    [Option(
        shortName: 't',
        longName: "GithubToken",
        Required = false,
        HelpText = "Github access token",
        Default = ""
    )]
    public string? GithubToken { get; init; }

    [Option(
        shortName: 'f',
        longName: "TagPrefix",
        Required = false,
        HelpText = "Git tag preifx",
        Default = ""
    )]
    public string? GitTagPrefix { get; init; }
}
