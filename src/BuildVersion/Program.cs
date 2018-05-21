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

        private const string RELEASE_PREFIX = "release/";
        private const string HOTFIX_PREFIX = "hotfix/";

        public static int Main()
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                List<string> branches = FindBranches();

                string currentBranch = FindCurrentBranch();
                int buildNumber = FindBuildNumber();
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

                NuGetVersion latest = new NuGetVersion("0.0.0.0");
                foreach (string branch in branches)
                {
                    Console.WriteLine($" * => {branch}");
                    if (IsReleaseBranch(branch))
                    {
                        NuGetVersion version = ExtractVersion(branch, buildNumber);
                        if (version != null)
                            if (latest < version)
                                latest = version;
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

            Console.WriteLine("Build Pre-Release Suffix: {usedSuffix}");

            Version version = new Version(latest.Version.Major, latest.Version.Minor, latest.Version.Build + 1, buildNumber);

            return new NuGetVersion(version, usedSuffix);
        }

        private static string BuildPreReleaseSuffix(string currentBranch)
        {
            StringBuilder suffix = new StringBuilder(currentBranch);
            foreach (char ch in currentBranch.Where(c => !char.IsLetterOrDigit(c))
                .Distinct()) suffix.Replace(ch, '-');

            suffix.Replace("--", "-");

            string usedSuffix = suffix.ToString()
                .ToLowerInvariant();

            if (string.IsNullOrWhiteSpace(usedSuffix)) usedSuffix = "prerelease";

            const int maxSuffixLength = 20;
            if (usedSuffix.Length > maxSuffixLength) usedSuffix = usedSuffix.Substring(0, maxSuffixLength);

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
            if (branch.StartsWith(prefix))
            {
                string version = branch.Substring(prefix.Length);
                if (NuGetVersion.TryParse(version, out NuGetVersion baseLine))
                {
                    Version dv = new Version(revision: buildNumber, build: baseLine.Version.Build, minor: baseLine.Version.Minor, major: baseLine.Version.Major);
                    return new NuGetVersion(dv);
                }
            }

            return null;
        }

        private static string FindCurrentBranch()
        {
            string branch = Environment.GetEnvironmentVariable(@"GIT_BRANCH");

            if (!string.IsNullOrWhiteSpace(branch))
            {
                Console.WriteLine($"GIT_BRANCH: {branch}");
                return branch.Trim();
            }

            string refs = File.ReadAllText(".git/HEAD")
                .Trim();

            Console.WriteLine($"Build Prefix: {refs}");

            const string prefix = "ref: refs/heads/";

            return refs.Substring(prefix.Length);
        }

        private static int FindBuildNumber()
        {
            string buildNumber = Environment.GetEnvironmentVariable(@"BUILD_NUMBER");
            if (!string.IsNullOrWhiteSpace(buildNumber))
                if (int.TryParse(buildNumber, out int build) && build >= 0)
                    return build;

            return 0;
        }

        private static bool IsReleaseBranch(string branchName)
        {
            if (branchName.StartsWith(RELEASE_PREFIX, StringComparison.OrdinalIgnoreCase)) return true;

            if (branchName.StartsWith(HOTFIX_PREFIX, StringComparison.OrdinalIgnoreCase)) return true;

            return false;
        }

        private static List<string> FindBranches()
        {
            List<string> branches = new List<string>();
            ProcessStartInfo psi = new ProcessStartInfo("git.exe", "branch --remote") {RedirectStandardOutput = true, CreateNoWindow = true};

            using (Process p = Process.Start(psi))
            {
                if (p == null) throw new Exception($"ERROR: Could not execute git");

                StreamReader s = p.StandardOutput;
                while (!s.EndOfStream)
                {
                    string line = p.StandardOutput.ReadLine();

                    string branch = ExtractBranch(line);
                    if (!string.IsNullOrWhiteSpace(branch)) branches.Add(branch);
                }

                p.WaitForExit();
            }

            return branches;
        }

        private static string ExtractBranch(string line)
        {
            string branch = line.Trim();
            if (line.StartsWith("origin/HEAD ", StringComparison.OrdinalIgnoreCase)) return null;

            const string originPrefix = "origin/";
            if (branch.StartsWith(originPrefix)) return branch.Substring(originPrefix.Length);

            return branch;
        }
    }
}