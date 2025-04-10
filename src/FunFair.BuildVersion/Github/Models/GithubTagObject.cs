using System.Diagnostics;
using System.Text.Json.Serialization;

namespace FunFair.BuildVersion.Github.Models;

[DebuggerDisplay("Type: {ObjectType} Sha: {Sha}")]
internal sealed class GithubTagObject
{
    [JsonConstructor]
    public GithubTagObject(string sha, string objectType, string url)
    {
        this.Sha = sha;
        this.ObjectType = objectType;
        this.Url = url;
    }

    [JsonPropertyName("sha")]
    public string Sha { get; }

    [JsonPropertyName("type")]
    public string ObjectType { get; }

    [JsonPropertyName("url")]
    public string Url { get; }
}
