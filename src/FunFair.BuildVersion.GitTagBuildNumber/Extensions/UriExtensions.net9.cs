#if NET9_0_OR_GREATER
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
#endif
