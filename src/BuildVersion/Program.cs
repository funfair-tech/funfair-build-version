using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NuGet.Versioning;

namespace BuildVersion
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        private const string RELEASE_PREFIX = @"release/";
        private const string HOTFIX_PREFIX = @"hotfix/";

        private const string PULL_REQUEST_PREFIX = @"/refs/pull/";
        private const string PULL_REQUEST_SUFFIX = @"/head";

        public static int Main(params string[] args)
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                string currentBranch = FindCurrentBranch();
                int buildNumber = FindBuildNumber(args.FirstOrDefault());
                Console.WriteLine($">>>>>> Current branch: {currentBranch}");
                Console.WriteLine($">>>>>> Current Build number: {buildNumber}");

                if (IsReleaseBranch(currentBranch))
                {
                    NuGetVersion version = ExtractVersion(currentBranch, buildNumber);

                    if (version == null)
                    {
                        Console.WriteLine($"Could not determine version number for {currentBranch}");

                        return ERROR;
                    }

                    ApplyVersion(version);

                    return SUCCESS;
                }

                List<string> branches = FindBranches();
                NuGetVersion latest = new NuGetVersion(version: @"0.0.0.0");

                foreach (string branch in branches)
                {
                    Console.WriteLine($" * => {branch}");

                    if (IsReleaseBranch(branch))
                    {
                        NuGetVersion version = ExtractVersion(branch, buildNumber);

                        if (version != null)
                        {
                            if (latest < version)
                            {
                                latest = version;
                            }
                        }
                    }
                }

                Console.WriteLine($"Latest Release Version: {latest}");

                NuGetVersion newVersion = BuildPreReleaseVersion(latest, currentBranch, buildNumber);
                ApplyVersion(newVersion);

                return SUCCESS;
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                return ERROR;
            }
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

            const char replacmentChar = '-';

            foreach (char ch in currentBranch.Where(predicate: c => !char.IsLetterOrDigit(c) && c != replacmentChar)
                .Distinct())
            {
                suffix.Replace(ch, replacmentChar);
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
        }

        private static NuGetVersion ExtractVersion(string branch, int buildNumber)
        {
            return ExtractVersionFromPrefix(branch, buildNumber, RELEASE_PREFIX) ?? ExtractVersionFromPrefix(branch, buildNumber, HOTFIX_PREFIX);
        }

        private static NuGetVersion ExtractVersionFromPrefix(string branch, int buildNumber, string prefix)
        {
            if (!branch.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            string version = branch.Substring(prefix.Length);

            if (!NuGetVersion.TryParse(version, out NuGetVersion baseLine))
            {
                return null;
            }

            Version dv = new Version(revision: buildNumber, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);

            return new NuGetVersion(dv);
        }

        private static string FindCurrentBranch()
        {
            string branch = Environment.GetEnvironmentVariable(variable: @"GIT_BRANCH");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                return ExtractBranchFromTeamCityBranchSpec(branch);
            }

            return ExtractBranchFromGitHead();
        }

        private static string ExtractBranchFromGitHead()
        {
            string refs = File.ReadAllText(path: ".git/HEAD")
                .Trim();

            Console.WriteLine($"Branch from Git head: {refs}");

            const string prefix = "ref: refs/heads/";

            return refs.Substring(prefix.Length);
        }

        private static string ExtractBranchFromTeamCityBranchSpec(string branch)
        {
            Console.WriteLine($"Branch from Teamcity: {branch}");
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

            string buildNumber = Environment.GetEnvironmentVariable(variable: @"BUILD_NUMBER");

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

        private static List<string> FindBranches()
        {
            Console.WriteLine(value: "Enumerating branches...");
            List<string> branches = new List<string>();
            ProcessStartInfo psi = new ProcessStartInfo(fileName: "git.exe", arguments: "branch --remote") {RedirectStandardOutput = true, CreateNoWindow = true};

            using (Process p = Process.Start(psi))
            {
                if (p == null)
                {
                    throw new FileNotFoundException($"ERROR: Could not execute {psi.FileName} {psi.Arguments}");
                }

                StreamReader s = p.StandardOutput;

                while (!s.EndOfStream)
                {
                    string line = p.StandardOutput.ReadLine();

                    string branch = ExtractBranch(line);

                    if (!string.IsNullOrWhiteSpace(branch))
                    {
                        branches.Add(branch);
                    }
                }

                p.WaitForExit();
            }

            return branches;
        }

        private static string ExtractBranch(string line)
        {
            string branch = line.Trim();

            if (line.StartsWith(value: "origin/HEAD ", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            const string originPrefix = "origin/";

            if (branch.StartsWith(originPrefix, StringComparison.OrdinalIgnoreCase))
            {
                return branch.Substring(originPrefix.Length);
            }

            return branch;
        }
    }
}