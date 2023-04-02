namespace FunFair.BuildVersion.Interfaces;

public interface IExternalBranchLocator
{
    string? CurrentBranch { get; }
}