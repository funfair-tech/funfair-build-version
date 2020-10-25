using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using NuGet.Versioning;
using Version = System.Version;

namespace FunFair.BuildVersion
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        public static int Main(params string[] args)
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                string workDir = Environment.CurrentDirectory;

                using (Repository repo = OpenRepository(workDir))
                {
                    string currentBranch = BranchDiscovery.FindCurrentBranch(repo);
                    int buildNumber = FindBuildNumber(args.FirstOrDefault());
                    Console.WriteLine($">>>>>> Current branch: {currentBranch}");
                    Console.WriteLine($">>>>>> Current Build number: {buildNumber}");

                    if (BranchClassification.IsReleaseBranch(currentBranch))
                    {
                        NuGetVersion? version = ExtractVersion(branch: currentBranch, buildNumber: buildNumber);

                        if (version == null)
                        {
                            Console.WriteLine($"Could not determine version number for {currentBranch}");

                            return ERROR;
                        }

                        ApplyVersion(version);

                        return SUCCESS;
                    }

                    NuGetVersion latest = DetermineLatestReleaseFromPreviousReleaseBranches(repo: repo, buildNumber: buildNumber);

                    Console.WriteLine($"Latest Release Version: {latest}");

                    NuGetVersion newVersion = BuildPreReleaseVersion(latest: latest, currentBranch: currentBranch, buildNumber: buildNumber);
                    ApplyVersion(newVersion);

                    return SUCCESS;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                return ERROR;
            }
        }

        private static NuGetVersion DetermineLatestReleaseFromPreviousReleaseBranches(Repository repo, int buildNumber)
        {
            List<string> branches = FindBranches(repo);
            NuGetVersion latest = new NuGetVersion(version: @"0.0.0.0");

            foreach (string branch in branches)
            {
                Console.WriteLine($" * => {branch}");

                if (BranchClassification.IsReleaseBranch(branch))
                {
                    NuGetVersion? version = ExtractVersion(branch: branch, buildNumber: buildNumber);

                    if (version != null)
                    {
                        if (latest < version)
                        {
                            latest = version;
                        }
                    }
                }
            }

            return latest;
        }

        private static Repository OpenRepository(string workDir)
        {
            string found = Repository.Discover(workDir);

            return new Repository(found);
        }

        private static NuGetVersion BuildPreReleaseVersion(NuGetVersion latest, string currentBranch, int buildNumber)
        {
            string usedSuffix = BuildPreReleaseSuffix(currentBranch);

            Console.WriteLine($"Build Pre-Release Suffix: {usedSuffix}");

            Version version = new Version(major: latest.Version.Major, minor: latest.Version.Minor, latest.Version.Build + 1, revision: buildNumber);

            return new NuGetVersion(version: version, releaseLabel: usedSuffix);
        }

        private static string BuildPreReleaseSuffix(string currentBranch)
        {
            if (PullRequest.ExtractPullRequestId(currentBranch: currentBranch, out long pullRequestId))
            {
                currentBranch = @"pull-request-" + pullRequestId.ToString(CultureInfo.InvariantCulture);
            }

            StringBuilder suffix = new StringBuilder(currentBranch);

            int pos = suffix.ToString()
                            .IndexOf('/');

            if (pos != -1)
            {
                suffix = suffix.Remove(startIndex: 0, pos + 1);
            }

            const char replacementChar = '-';

            foreach (char ch in currentBranch.Where(predicate: c => !char.IsLetterOrDigit(c) && c != replacementChar)
                                             .Distinct())
            {
                suffix.Replace(oldChar: ch, newChar: replacementChar);
            }

            suffix.Replace(oldValue: "--", newValue: "-");

            string usedSuffix = suffix.ToString()
                                      .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(usedSuffix))
            {
                usedSuffix = @"prerelease";
            }

            usedSuffix = usedSuffix.TrimStart('-');

            const int maxSuffixLength = 15;

            if (usedSuffix.Length > maxSuffixLength)
            {
                usedSuffix = usedSuffix.Substring(startIndex: 0, length: maxSuffixLength);
            }

            // Ensure that the name doesn't end with a -
            usedSuffix = usedSuffix.Trim(trimChar: '-');

            return usedSuffix;
        }

        private static void ApplyVersion(NuGetVersion version)
        {
            Console.WriteLine($"Version: {version}");
            ApplyTeamCityVersion(version);
            ApplyGithubActionsVersion(version);
        }

        private static void ApplyGithubActionsVersion(NuGetVersion version)
        {
            string? env = Environment.GetEnvironmentVariable("GITHUB_ENV");

            Console.WriteLine($"Github: {env ?? string.Empty}");

            if (!string.IsNullOrEmpty(env))
            {
                File.AppendAllLines(path: env, new[] {$"::set-env name=BUILD_VERSION::{version}"});
            }
        }

        private static void ApplyTeamCityVersion(NuGetVersion version)
        {
            string? env = Environment.GetEnvironmentVariable("TEAMCITY_VERSION");

            Console.WriteLine($"TeamCity: {env ?? string.Empty}");

            if (!string.IsNullOrWhiteSpace(env))
            {
                Console.WriteLine($"##teamcity[buildNumber '{version}']");
                Console.WriteLine($"##teamcity[setParameter name='system.build.version' value='{version}']");
            }
        }

        private static NuGetVersion? ExtractVersion(string branch, int buildNumber)
        {
            return ExtractVersionFromPrefix(branch: branch, buildNumber: buildNumber, prefix: BranchClassification.ReleasePrefix) ??
                   ExtractVersionFromPrefix(branch: branch, buildNumber: buildNumber, prefix: BranchClassification.HotfixPrefix);
        }

        private static NuGetVersion? ExtractVersionFromPrefix(string branch, int buildNumber, string prefix)
        {
            if (!branch.StartsWith(value: prefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string version = branch.Substring(prefix.Length);

            if (!NuGetVersion.TryParse(value: version, out NuGetVersion? baseLine))
            {
                return null;
            }

            Version dv = new Version(revision: buildNumber, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);

            return new NuGetVersion(dv);
        }

        private static int FindBuildNumber(string? buildNumberFromCommandLine)
        {
            if (!string.IsNullOrWhiteSpace(buildNumberFromCommandLine))
            {
                Console.WriteLine($"Build number from command line: {buildNumberFromCommandLine}");

                if (int.TryParse(s: buildNumberFromCommandLine, out int build) && build >= 0)
                {
                    return build;
                }
            }

            string? buildNumber = Environment.GetEnvironmentVariable(variable: @"BUILD_NUMBER");

            if (!string.IsNullOrWhiteSpace(buildNumber))
            {
                Console.WriteLine($"Build number from TeamCity: {buildNumberFromCommandLine}");

                if (int.TryParse(s: buildNumber, out int build) && build >= 0)
                {
                    return build;
                }
            }

            return 0;
        }

        private static List<string> FindBranches(Repository repository)
        {
            Console.WriteLine(value: "Enumerating branches...");

            return repository.Branches.Select(selector: b => ExtractBranch(b.FriendlyName))
                             .ToList();
        }

        private static string ExtractBranch(string branch)
        {
            const string originPrefix = "origin/";

            if (branch.StartsWith(value: originPrefix, comparisonType: StringComparison.OrdinalIgnoreCase))
            {
                return branch.Substring(originPrefix.Length);
            }

            return branch;
        }
    }
}