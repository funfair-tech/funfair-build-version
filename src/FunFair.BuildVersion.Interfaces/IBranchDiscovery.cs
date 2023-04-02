using System.Collections.Generic;

namespace FunFair.BuildVersion.Interfaces;

public interface IBranchDiscovery
{
    string FindCurrentBranch();

    IReadOnlyList<string> FindBranches();
}