using System.Diagnostics;

namespace FunFair.BuildVersion.Github;

[DebuggerDisplay("Repository: {Repository} Sha: {Sha}")]
public readonly record struct GitHubContext(
    string Token,
    string Repository,
    string Sha,
    string Prefix
);
