using System.Diagnostics;
using System.Text.Json.Serialization;

namespace FunFair.BuildVersion.GitTagBuildNumber.Github.Models;

[DebuggerDisplay("Ref: {Reference} NodeId: {NodeId}")]
internal sealed class GithubTagReference
{
    [JsonConstructor]
    public GithubTagReference(string reference, string nodeId, string url, GithubTagObject obj)
    {
        this.Reference = reference;
        this.NodeId = nodeId;
        this.Url = url;
        this.Obj = obj;
    }

    [JsonPropertyName("ref")]
    public string Reference { get; }

    [JsonPropertyName("node_id")]
    public string NodeId { get; }

    [JsonPropertyName("url")]
    public string Url { get; }

    [JsonPropertyName("object")]
    public GithubTagObject Obj { get; }
}