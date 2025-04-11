using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using FunFair.BuildVersion.GitTagBuildNumber.Helpers;

namespace FunFair.BuildVersion.GitTagBuildNumber.Github;

public static class RepoUrlParser
{
    public static bool TryParse(
        string path,
        out GitUrlProtocol protocol,
        [NotNullWhen(true)] out string? host,
        [NotNullWhen(true)] out string? repo
    )
    {
        if (Uri.TryCreate(uriString: path, uriKind: UriKind.Absolute, out Uri? uri))
        {
            if (IsHttp(uri))
            {
                protocol = GitUrlProtocol.HTTP;
                host = uri.Host;
                repo = RemoveDotGit(uri.AbsolutePath);

                return true;
            }

            protocol = GitUrlProtocol.UNKNOWN;
            host = null;
            repo = null;

            return false;
        }

        Match m = GitUrlProtocolRegex.SshHostAndRepo().Match(path);

        if (m.Success)
        {
            protocol = GitUrlProtocol.SSH;
            host = m.Groups["Host"].Value;
            repo = m.Groups["Repo"].Value;

            return true;
        }

        protocol = GitUrlProtocol.UNKNOWN;
        host = null;
        repo = null;

        return false;
    }

    private static string RemoveDotGit(string path)
    {
        return path.EndsWith(value: ".git", comparisonType: StringComparison.OrdinalIgnoreCase)
            ? path[1..^".git".Length]
            : path[1..];
    }

    private static bool IsHttp(Uri uri)
    {
        return StringComparer.Ordinal.Equals(x: uri.Scheme, y: "https")
            || StringComparer.Ordinal.Equals(x: uri.Scheme, y: "http");
    }
}
