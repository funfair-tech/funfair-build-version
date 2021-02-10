using System;

namespace FunFair.BuildVersion.Detection
{
    public static class BranchSpecExtractor
    {
        public static string ExtractBranchFromBranchSpec(string branch)
        {
            Console.WriteLine($"Branch from CI: {branch}");
            string branchRef = branch.Trim();

            const string branchRefPrefix = "refs/heads/";

            if (branchRef.StartsWith(value: branchRefPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                branchRef = branchRef.Substring(branchRefPrefix.Length);
            }

            return branchRef;
        }
    }
}