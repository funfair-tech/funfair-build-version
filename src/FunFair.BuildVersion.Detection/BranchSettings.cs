using System.Diagnostics;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection;

[DebuggerDisplay("Suffix: {ReleaseSuffix} Package: {Package}")]
public sealed class BranchSettings : IBranchSettings
{
    public BranchSettings(string? releaseSuffix, string? package)
    {
        this.ReleaseSuffix = releaseSuffix;
        this.Package = package;
    }

    public string? ReleaseSuffix { get; }

    public string? Package { get; }
}
