using System;
using System.Buffers;

namespace FunFair.BuildVersion.GitTagBuildNumber.Extensions;

public static partial class UriExtensions
{
    private static readonly SearchValues<string> HttpProtocols = SearchValues.Create(
        [INSECURE, SECURE],
        comparisonType: StringComparison.Ordinal
    );

    public static bool IsHttp(this Uri uri)
    {
        return HttpProtocols.Contains(uri.Scheme);
    }
}
