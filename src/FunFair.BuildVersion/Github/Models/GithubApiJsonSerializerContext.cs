using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace FunFair.BuildVersion.Github.Models;

[JsonSourceGenerationOptions(
    GenerationMode = JsonSourceGenerationMode.Serialization | JsonSourceGenerationMode.Metadata,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    IncludeFields = false
)]
[JsonSerializable(typeof(IReadOnlyList<GithubTagReference>))]
[JsonSerializable(typeof(GithubTagReference))]
[JsonSerializable(typeof(GithubTagObject))]
[JsonSerializable(typeof(GithubNewTagRef))]
[SuppressMessage(
    category: "ReSharper",
    checkId: "PartialTypeWithSinglePart",
    Justification = "Required for " + nameof(JsonSerializerContext) + " code generation"
)]
internal sealed partial class GithubApiJsonSerializerContext : JsonSerializerContext;
