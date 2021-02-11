using System.Collections.Generic;

namespace FunFair.BuildVersion.Interfaces
{
    /// <summary>
    ///     Branch discovery
    /// </summary>
    public interface IBranchDiscovery
    {
        /// <summary>
        ///     Finds the current branch.
        /// </summary>
        /// <returns></returns>
        string FindCurrentBranch();

        /// <summary>
        ///     Find the branches that have been created in the repository.
        /// </summary>
        /// <returns>The branch names</returns>
        IReadOnlyList<string> FindBranches();
    }
}