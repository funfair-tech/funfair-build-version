#if NET9_0_OR_GREATER
#else
using System;

namespace FunFair.BuildVersion.GitTagBuildNumber.Extensions;

public static partial class UriExtensions
{
    public static bool IsHttp(this Uri uri)
    {
        return StringComparer.Ordinal.Equals(x: uri.Scheme, y: INSECURE) || StringComparer.Ordinal.Equals(x: uri.Scheme, y: SECURE);
    }
}
#endif