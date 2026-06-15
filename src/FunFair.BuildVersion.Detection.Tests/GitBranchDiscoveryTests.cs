using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using FunFair.Test.Infrastructure.Mocks;
using LibGit2Sharp;
using NSubstitute;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests;

public sealed class GitBranchDiscoveryTests : LoggingFolderCleanupTestBase
{
    public GitBranchDiscoveryTests(ITestOutputHelper output)
        : base(output) { }

    [Fact]
    public async Task FindCurrentBranch_WithExternalLocatorReturningBranch_AndNotPullRequest_ReturnsThatBranch()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);

        IBranchClassification branchClassification = GetSubstitute<IBranchClassification>();
        IExternalBranchLocator externalLocator = GetSubstitute<IExternalBranchLocator>();

        externalLocator.CurrentBranch.Returns("feature/test");
        branchClassification.IsPullRequest(currentBranch: "feature/test", out Arg.Any<long>()).Returns(false);

        GitBranchDiscovery discovery = new(
            branchClassification: branchClassification,
            externalBranchLocators: [externalLocator],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        string result = discovery.FindCurrentBranch(repo);

        Assert.Equal(expected: "feature/test", actual: result);
    }

    [Fact]
    public async Task FindCurrentBranch_WithNoExternalLocators_FallsBackToGitHead_WhenNotPullRequest()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);
        string headBranchName = repo.Head.FriendlyName;

        IBranchClassification branchClassification = GetSubstitute<IBranchClassification>();
        branchClassification.IsPullRequest(currentBranch: headBranchName, out Arg.Any<long>()).Returns(false);

        GitBranchDiscovery discovery = new(
            branchClassification: branchClassification,
            externalBranchLocators: [],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        string result = discovery.FindCurrentBranch(repo);

        Assert.Equal(expected: headBranchName, actual: result);
    }

    [Fact]
    public async Task FindCurrentBranch_WhenPullRequestBranchWithNoMatchingCandidate_ReturnsOriginalBranch()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);
        string headBranchName = repo.Head.FriendlyName;

        IBranchClassification branchClassification = GetSubstitute<IBranchClassification>();
        IExternalBranchLocator externalLocator = GetSubstitute<IExternalBranchLocator>();

        // External locator returns same name as the actual HEAD branch,
        // so IsMatchingPullRequestBranch will not find a candidate with a different name
        externalLocator.CurrentBranch.Returns(headBranchName);
        MockBranchClassificationIsPullRequest(
            branchClassification: branchClassification,
            currentBranch: headBranchName,
            pullRequestId: 42L
        );

        GitBranchDiscovery discovery = new(
            branchClassification: branchClassification,
            externalBranchLocators: [externalLocator],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        string result = discovery.FindCurrentBranch(repo);

        Assert.Equal(expected: headBranchName, actual: result);
    }

    [Fact]
    public async Task FindCurrentBranch_WhenPullRequestBranchWithMatchingNonReleaseCandidate_ReturnsCandidateBranchName()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);
        string headBranchName = repo.Head.FriendlyName;

        IBranchClassification branchClassification = GetSubstitute<IBranchClassification>();
        IExternalBranchLocator externalLocator = GetSubstitute<IExternalBranchLocator>();

        // External locator returns a name different from the actual HEAD branch,
        // so the HEAD branch will be found as a matching candidate (same SHA, different name)
        externalLocator.CurrentBranch.Returns("pr-42");
        MockBranchClassificationIsPullRequest(
            branchClassification: branchClassification,
            currentBranch: "pr-42",
            pullRequestId: 42L
        );
        branchClassification.IsRelease(branchName: headBranchName, out Arg.Any<NuGetVersion?>()).Returns(false);

        GitBranchDiscovery discovery = new(
            branchClassification: branchClassification,
            externalBranchLocators: [externalLocator],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        string result = discovery.FindCurrentBranch(repo);

        Assert.Equal(expected: headBranchName, actual: result);
    }

    [Fact]
    public async Task FindCurrentBranch_WhenPullRequestBranchWithMatchingReleaseCandidate_ReturnsPrefixedCandidateBranchName()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);
        string headBranchName = repo.Head.FriendlyName;

        IBranchClassification branchClassification = GetSubstitute<IBranchClassification>();
        IExternalBranchLocator externalLocator = GetSubstitute<IExternalBranchLocator>();

        externalLocator.CurrentBranch.Returns("pr-42");
        MockBranchClassificationIsPullRequest(
            branchClassification: branchClassification,
            currentBranch: "pr-42",
            pullRequestId: 42L
        );
        branchClassification
            .IsRelease(branchName: headBranchName, out Arg.Any<NuGetVersion?>())
            .Returns(x =>
            {
                x[1] = new NuGetVersion("1.0.0");

                return true;
            });

        GitBranchDiscovery discovery = new(
            branchClassification: branchClassification,
            externalBranchLocators: [externalLocator],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        string result = discovery.FindCurrentBranch(repo);

        Assert.Equal(expected: "pre-" + headBranchName, actual: result);
    }

    [Fact]
    public void FindBranches_WhenRepoHasNoBranches_ReturnsEmptyList()
    {
        string emptyRepoPath = Path.Combine(this.TempFolder, "empty");
        Directory.CreateDirectory(emptyRepoPath);
        Repository.Init(emptyRepoPath);

        using Repository repo = new(emptyRepoPath);

        GitBranchDiscovery discovery = new(
            branchClassification: GetSubstitute<IBranchClassification>(),
            externalBranchLocators: [],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        IReadOnlyList<string> branches = discovery.FindBranches(repo);

        Assert.Empty(branches);
    }

    [Fact]
    public async Task FindBranches_WhenRepoHasLocalBranchAndNoRemotes_ReturnsBranchName()
    {
        using Repository repo = await CreateRepoWithCommitAsync(this.TempFolder);
        string headBranchName = repo.Head.FriendlyName;

        GitBranchDiscovery discovery = new(
            branchClassification: GetSubstitute<IBranchClassification>(),
            externalBranchLocators: [],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        IReadOnlyList<string> branches = discovery.FindBranches(repo);

        Assert.Contains(expected: headBranchName, collection: branches);
    }

    [Fact]
    public async Task FindBranches_WhenRepoHasBranchWithRemotePrefix_StripsRemotePrefix()
    {
        string serverPath = Path.Combine(this.TempFolder, "server");
        string clientPath = Path.Combine(this.TempFolder, "client");
        Directory.CreateDirectory(serverPath);

        using (Repository serverRepo = new(Repository.Init(serverPath)))
        {
            await CreateCommitInRepoAsync(serverRepo);
        }

        Repository.Clone(sourceUrl: serverPath, workdirPath: clientPath);

        using Repository clientRepo = new(clientPath);

        GitBranchDiscovery discovery = new(
            branchClassification: GetSubstitute<IBranchClassification>(),
            externalBranchLocators: [],
            logger: this.GetTypedLogger<GitBranchDiscovery>()
        );

        IReadOnlyList<string> branches = discovery.FindBranches(clientRepo);

        Assert.DoesNotContain(
            collection: branches,
            filter: static b => b.StartsWith(value: "origin/", comparisonType: StringComparison.OrdinalIgnoreCase)
        );
    }

    private static void MockBranchClassificationIsPullRequest(
        IBranchClassification branchClassification,
        string currentBranch,
        long pullRequestId
    )
    {
        branchClassification
            .IsPullRequest(currentBranch: currentBranch, out Arg.Any<long>())
            .Returns(x =>
            {
                x[1] = pullRequestId;

                return true;
            });
    }

    private static async Task<Repository> CreateRepoWithCommitAsync(string path)
    {
        Repository.Init(path);
        Repository repo = new(path);
        await CreateCommitInRepoAsync(repo);

        return repo;
    }

    private static async Task CreateCommitInRepoAsync(Repository repo)
    {
        string filePath = Path.Combine(repo.Info.WorkingDirectory, "README.md");
        await File.WriteAllTextAsync(
            path: filePath,
            contents: "test",
            cancellationToken: TestContext.Current.CancellationToken
        );
        repo.Index.Add("README.md");
        repo.Index.Write();
        Signature sig = new(name: "test", email: "test@test.com", when: MockDateTimeSources.Past.Start);
        repo.Commit(message: "Initial commit", author: sig, committer: sig);
    }
}
