using System.Diagnostics;
using System.Text.Json.Serialization;

namespace FunFair.BuildVersion.GitTagBuildNumber.Github.Models;

[DebuggerDisplay("Ref: {Reference} Sha: {Sha}")]
internal sealed class GithubNewTagRef
{
    [JsonConstructor]
    public GithubNewTagRef(string reference, string sha)
    {
        this.Reference = reference;
        this.Sha = sha;
    }

    [JsonPropertyName("ref")]
    public string Reference { get; }

    [JsonPropertyName("sha")]
    public string Sha { get; }
}