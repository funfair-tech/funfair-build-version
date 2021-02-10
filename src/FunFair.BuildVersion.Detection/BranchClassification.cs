using System;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection
{
    public sealed class BranchClassification : IBranchClassification
    {
        public const string ReleasePrefix = @"release/";
        public const string HotfixPrefix = @"hotfix/";

        /// <inheritdoc />
        public bool IsReleaseBranch(string branchName)
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