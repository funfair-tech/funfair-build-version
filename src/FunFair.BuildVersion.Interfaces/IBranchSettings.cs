namespace FunFair.BuildVersion.Interfaces;

public interface IBranchSettings
{
    string? ReleaseSuffix { get; }

    string? Package { get; }
}