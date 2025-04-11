using System;
using FunFair.BuildVersion.Detection.ExternalBranchLocators.LoggingExtensions;
using FunFair.BuildVersion.Interfaces;
using Microsoft.Extensions.Logging;

namespace FunFair.BuildVersion.Detection.ExternalBranchLocators;

public abstract class EnvironmentVariableBranchLocator : IExternalBranchLocator
{
    protected EnvironmentVariableBranchLocator(string environmentVariable, ILogger logger)
    {
        this.CurrentBranch = ExtractBranch(
            environmentVariableName: environmentVariable,
            logger: logger
        );
    }

    public string? CurrentBranch { get; }

    private static string? ExtractBranch(string environmentVariableName, ILogger logger)
    {
        string? branch = Environment.GetEnvironmentVariable(variable: environmentVariableName);

        if (string.IsNullOrWhiteSpace(branch))
        {
            return null;
        }

        logger.LogBranchFromContinuousIntegration(environmentVariableName, branch);
        string branchRef = branch.Trim();

        const string branchRefPrefix = "refs/heads/";

        if (
            branchRef.StartsWith(
                value: branchRefPrefix,
                comparisonType: StringComparison.OrdinalIgnoreCase
            )
        )
        {
            branchRef = branchRef[branchRefPrefix.Length..];
        }

        return branchRef;
    }
}
