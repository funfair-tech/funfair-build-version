using System;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators
{
    /// <summary>
    ///     Branch locator that uses environment variables
    /// </summary>
    public abstract class EnvironmentVariableBranchLocator : IExternalBranchLocator
    {
        /// <summary>
        ///     Constructor.
        /// </summary>
        /// <param name="environmentVariable">The environment variable to read</param>
        protected EnvironmentVariableBranchLocator(string environmentVariable)
        {
            this.CurrentBranch = ExtractBranch(environmentVariable);
        }

        /// <inheritdoc />
        public string? CurrentBranch { get; }

        private static string? ExtractBranch(string environmentVariableName)
        {
            string? branch = Environment.GetEnvironmentVariable(variable: environmentVariableName);

            if (string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            return BranchSpecExtractor.ExtractBranchFromBranchSpec(branch);
        }
    }
}