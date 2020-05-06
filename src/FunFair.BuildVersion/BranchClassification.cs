using System;

namespace FunFair.BuildVersion
{
    internal static class BranchClassification
    {
        public const string ReleasePrefix = @"release/";
        public const string HotfixPrefix = @"hotfix/";

        public static bool IsReleaseBranch(string branchName)
        {
            if (branchName.StartsWith(value: ReleasePrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (branchName.StartsWith(value: HotfixPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
        }
    }
}