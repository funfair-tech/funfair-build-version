namespace FunFair.BuildVersion.Interfaces;

/// <summary>
///     Locates the current branch without using non-repository based hints.
/// </summary>
public interface IExternalBranchLocator
{
    /// <summary>
    ///     Locates the current branch without using non-repository based hints.
    /// </summary>
    string? CurrentBranch { get; }
}