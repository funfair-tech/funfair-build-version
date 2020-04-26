using System;
using System.Collections.Generic;
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

        private const string RELEASE_PREFIX = @"release/";
        private const string HOTFIX_PREFIX = @"hotfix/";

        private const string PULL_REQUEST_PREFIX = @"refs/pull/";
        private const string PULL_REQUEST_SUFFIX = @"/head";

        public static int Main(params string[] args)
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                string workDir = Environment.CurrentDirectory;

                using (Repository repo = OpenRepository(workDir))
                {
                    string currentBranch = FindCurrentBranch(repo);
                    int buildNumber = FindBuildNumber(args.FirstOrDefault());
                    Console.WriteLine($">>>>>> Current branch: {currentBranch}");
                    Console.WriteLine($">>>>>> Current Build number: {buildNumber}");

                    if (IsReleaseBranch(currentBranch))
                    {
                        NuGetVersion? version = ExtractVersion(currentBranch, buildNumber);

                        if (version == null)
                        {
                            Console.WriteLine($"Could not determine version number for {currentBranch}");

                            return ERROR;
                        }

                        ApplyVersion(version);

                        return SUCCESS;
                    }

                    NuGetVersion latest = DetermineLatestReleaseFromPreviousReleaseBranches(repo, buildNumber);

                    Console.WriteLine($"Latest Release Version: {latest}");

                    NuGetVersion newVersion = BuildPreReleaseVersion(latest, currentBranch, buildNumber);
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

                if (IsReleaseBranch(branch))
                {
                    NuGetVersion? version = ExtractVersion(branch, buildNumber);

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

            Version version = new Version(latest.Version.Major, latest.Version.Minor, latest.Version.Build + 1, buildNumber);

            return new NuGetVersion(version, usedSuffix);
        }

        private static string BuildPreReleaseSuffix(string currentBranch)
        {
            if (currentBranch.StartsWith(PULL_REQUEST_PREFIX, StringComparison.Ordinal))
            {
                currentBranch = currentBranch.Substring(PULL_REQUEST_PREFIX.Length);

                if (currentBranch.EndsWith(PULL_REQUEST_SUFFIX, StringComparison.Ordinal))
                {
                    currentBranch = currentBranch.Substring(startIndex: 0, currentBranch.Length - PULL_REQUEST_SUFFIX.Length);
                }

                currentBranch = @"pull-request-" + currentBranch;
            }

            StringBuilder suffix = new StringBuilder(currentBranch);

            const char replacementChar = '-';

            foreach (char ch in currentBranch.Where(predicate: c => !char.IsLetterOrDigit(c) && c != replacementChar)
                                             .Distinct())
            {
                suffix.Replace(ch, replacementChar);
            }

            suffix.Replace(oldValue: "--", newValue: "-");

            string usedSuffix = suffix.ToString()
                                      .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(usedSuffix))
            {
                usedSuffix = @"prerelease";
            }

            const int maxSuffixLength = 20;

            if (usedSuffix.Length > maxSuffixLength)
            {
                usedSuffix = usedSuffix.Substring(startIndex: 0, maxSuffixLength);
            }

            // Ensure that the name doesn't end with a -
            usedSuffix = usedSuffix.TrimEnd(trimChar: '-');

            return usedSuffix;
        }

        private static void ApplyVersion(NuGetVersion version)
        {
            Console.WriteLine($"Version: {version}");
            Console.WriteLine($"##teamcity[buildNumber '{version}']");
            Console.WriteLine($"##teamcity[setParameter name='system.build.version' value='{version}']");
            Console.WriteLine($"::set-env name=BUILD_VERSION::{version}");
        }

        private static NuGetVersion? ExtractVersion(string branch, int buildNumber)
        {
            return ExtractVersionFromPrefix(branch, buildNumber, RELEASE_PREFIX) ?? ExtractVersionFromPrefix(branch, buildNumber, HOTFIX_PREFIX);
        }

        private static NuGetVersion? ExtractVersionFromPrefix(string branch, int buildNumber, string prefix)
        {
            if (!branch.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string version = branch.Substring(prefix.Length);

            if (!NuGetVersion.TryParse(version, out NuGetVersion? baseLine))
            {
                return null;
            }

            Version dv = new Version(revision: buildNumber, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);

            return new NuGetVersion(dv);
        }

        private static string FindCurrentBranch(Repository repo)
        {
            string? branch = Environment.GetEnvironmentVariable(variable: @"GIT_BRANCH");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                return ExtractBranchFromBranchSpec(branch);
            }

            branch = Environment.GetEnvironmentVariable(variable: @"GITHUB_REF");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                return ExtractBranchFromBranchSpec(branch);
            }

            return ExtractBranchFromGitHead(repo);
        }

        private static string ExtractBranchFromGitHead(Repository repository)
        {
            return repository.Head.FriendlyName;
        }

        private static string ExtractBranchFromBranchSpec(string branch)
        {
            Console.WriteLine($"Branch from CI: {branch}");
            string branchRef = branch.Trim();

            const string branchRefPrefix = "refs/heads/";

            if (branchRef.StartsWith(branchRefPrefix, StringComparison.OrdinalIgnoreCase))
            {
                branchRef = branchRef.Substring(branchRefPrefix.Length);
            }

            return branchRef;
        }

        private static int FindBuildNumber(string buildNumberFromCommandLine)
        {
            if (!string.IsNullOrWhiteSpace(buildNumberFromCommandLine))
            {
                Console.WriteLine($"Build number from command line: {buildNumberFromCommandLine}");

                if (int.TryParse(buildNumberFromCommandLine, out int build) && build >= 0)
                {
                    return build;
                }
            }

            string? buildNumber = Environment.GetEnvironmentVariable(variable: @"BUILD_NUMBER");

            if (!string.IsNullOrWhiteSpace(buildNumber))
            {
                Console.WriteLine($"Build number from TeamCity: {buildNumberFromCommandLine}");

                if (int.TryParse(buildNumber, out int build) && build >= 0)
                {
                    return build;
                }
            }

            return 0;
        }

        private static bool IsReleaseBranch(string branchName)
        {
            if (branchName.StartsWith(RELEASE_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (branchName.StartsWith(HOTFIX_PREFIX, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return false;
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

            if (branch.StartsWith(originPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return branch.Substring(originPrefix.Length);
            }

            return branch;
        }
    }
}