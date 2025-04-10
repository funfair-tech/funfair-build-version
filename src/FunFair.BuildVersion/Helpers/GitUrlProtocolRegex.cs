using System.Text.RegularExpressions;

namespace FunFair.BuildVersion.Helpers;

internal static partial class GitUrlProtocolRegex
{
    private const RegexOptions REGEX_OPTIONS =
        RegexOptions.Compiled
        | RegexOptions.CultureInvariant
        | RegexOptions.ExplicitCapture
        | RegexOptions.Singleline
        | RegexOptions.NonBacktracking;

    private const int REGEX_TIMEOUT_MILLISECONDS = 1000;

    [GeneratedRegex(
        pattern: "^(.*)@(?<Host>.*):(?<Repo>.*?)(\\.git)?$",
        REGEX_OPTIONS,
        matchTimeoutMilliseconds: REGEX_TIMEOUT_MILLISECONDS
    )]
    public static partial Regex SshHostAndRepo();

    [GeneratedRegex(
        pattern: "\\-(?<BuildNumber>\\d*)$",
        REGEX_OPTIONS,
        matchTimeoutMilliseconds: REGEX_TIMEOUT_MILLISECONDS
    )]
    public static partial Regex BuildNumbersFromTag();
}
