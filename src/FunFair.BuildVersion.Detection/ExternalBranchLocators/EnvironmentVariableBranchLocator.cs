using System;
using FunFair.BuildVersion.Interfaces;
using Microsoft.Extensions.Logging;

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
        /// <param name="logger">Logging.</param>
        protected EnvironmentVariableBranchLocator(string environmentVariable, ILogger logger)
        {
            this.CurrentBranch = ExtractBranch(environmentVariableName: environmentVariable, logger: logger);
        }

        /// <inheritdoc />
        public string? CurrentBranch { get; }

        private static string? ExtractBranch(string environmentVariableName, ILogger logger)
        {
            string? branch = Environment.GetEnvironmentVariable(variable: environmentVariableName);

            if (string.IsNullOrWhiteSpace(branch))
            {
                return null;
            }

            logger.LogInformation($"Branch from CI: {branch}");
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