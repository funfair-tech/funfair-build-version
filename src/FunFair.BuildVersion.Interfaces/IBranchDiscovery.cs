using System.Collections.Generic;
using LibGit2Sharp;

namespace FunFair.BuildVersion.Interfaces;

public interface IBranchDiscovery
{
    string FindCurrentBranch(Repository repository);

    IReadOnlyList<string> FindBranches(Repository repository);
}
