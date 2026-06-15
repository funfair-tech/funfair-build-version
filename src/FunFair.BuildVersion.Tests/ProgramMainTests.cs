using System;
using System.IO;
using System.Threading.Tasks;
using FunFair.BuildVersion.Tests.Helpers;
using FunFair.Test.Common;
using FunFair.Test.Infrastructure.Mocks;
using LibGit2Sharp;
using Xunit;

namespace FunFair.BuildVersion.Tests;

[Collection("ProgramMainTests")]
public sealed class ProgramMainTests : TestBase
{
    private static readonly string[] UnrecognisedArgs = ["--definitely-not-a-valid-flag"];
    private static readonly string[] ValidBuildNumberArgs = ["--BuildNumber", "1"];

    [Fact]
    public async ValueTask MainWithUnrecognisedArgs_ReturnsError()
    {
        using ConsoleCapture capture = new();

        int result = await FunFair.BuildVersion.Program.Main(UnrecognisedArgs);

        Assert.Equal(expected: 1, actual: result);
        Assert.Contains(
            expectedSubstring: "Errors:",
            actualString: capture.StdOut,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async ValueTask MainWithValidBuildNumber_InGitRepo_ReturnsSuccess()
    {
        using ConsoleCapture capture = new();

        int result = await FunFair.BuildVersion.Program.Main(ValidBuildNumberArgs);

        Assert.Equal(expected: 0, actual: result);
    }

    [Fact]
    public async ValueTask MainWithExceptionFromOpenRepository_CatchesAndReturnsError()
    {
        string originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = Path.GetTempPath();

            using ConsoleCapture capture = new();

            int result = await FunFair.BuildVersion.Program.Main(ValidBuildNumberArgs);

            Assert.Equal(expected: 1, actual: result);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
        }
    }

    [Fact]
    public async ValueTask MainWithCorruptGitRepo_CatchesAndReturnsError()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            // Create a .git directory with only the HEAD file pointing nowhere,
            // causing LibGit2Sharp to throw when trying to open the repository.
            string gitDir = Path.Combine(tempDir, ".git");
            Directory.CreateDirectory(gitDir);
            await File.WriteAllTextAsync(
                path: Path.Combine(gitDir, "HEAD"),
                contents: "ref: refs/heads/main\n",
                cancellationToken: TestContext.Current.CancellationToken
            );

            string originalDirectory = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = tempDir;

                using ConsoleCapture capture = new();

                int result = await FunFair.BuildVersion.Program.Main(ValidBuildNumberArgs);

                Assert.Equal(expected: 1, actual: result);
                Assert.Contains(
                    expectedSubstring: "ERROR:",
                    actualString: capture.StdOut,
                    comparisonType: StringComparison.Ordinal
                );
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }
        finally
        {
            Directory.Delete(path: tempDir, recursive: true);
        }
    }

    [Fact]
    public async ValueTask MainWithGithubTokenAndNoOriginRemote_ReturnsSuccess()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            Repository.Init(tempDir);

            using (Repository tempRepo = new(tempDir))
            {
                string filePath = Path.Combine(tempDir, "README.md");
                await File.WriteAllTextAsync(
                    path: filePath,
                    contents: "test",
                    cancellationToken: TestContext.Current.CancellationToken
                );
                tempRepo.Index.Add("README.md");
                tempRepo.Index.Write();
                Signature sig = new(name: "test", email: "test@test.com", when: MockDateTimeSources.Past.Start);
                tempRepo.Commit(message: "Initial commit", author: sig, committer: sig);
            }

            string originalDirectory = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = tempDir;

                using ConsoleCapture capture = new();

                int result = await FunFair.BuildVersion.Program.Main(
                    "--GithubToken",
                    "test-token-xyz",
                    "--BuildNumber",
                    "1"
                );

                Assert.Equal(expected: 0, actual: result);
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }
        finally
        {
            Directory.Delete(path: tempDir, recursive: true);
        }
    }

    [Fact]
    public async ValueTask MainWithGithubTokenAndNonGithubRemote_ReturnsSuccess()
    {
        string tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(tempDir);

        try
        {
            Repository.Init(tempDir);

            using (Repository tempRepo = new(tempDir))
            {
                string filePath = Path.Combine(tempDir, "README.md");
                await File.WriteAllTextAsync(
                    path: filePath,
                    contents: "test",
                    cancellationToken: TestContext.Current.CancellationToken
                );
                tempRepo.Index.Add("README.md");
                tempRepo.Index.Write();
                Signature sig = new(name: "test", email: "test@test.com", when: MockDateTimeSources.Past.Start);
                tempRepo.Commit(message: "Initial commit", author: sig, committer: sig);
                _ = tempRepo.Network.Remotes.Add(name: "origin", url: "https://example.com/user/repo");
            }

            string originalDirectory = Environment.CurrentDirectory;

            try
            {
                Environment.CurrentDirectory = tempDir;

                using ConsoleCapture capture = new();

                int result = await FunFair.BuildVersion.Program.Main(
                    "--GithubToken",
                    "test-token-xyz",
                    "--BuildNumber",
                    "1"
                );

                Assert.Equal(expected: 0, actual: result);
            }
            finally
            {
                Environment.CurrentDirectory = originalDirectory;
            }
        }
        finally
        {
            Directory.Delete(path: tempDir, recursive: true);
        }
    }
}
