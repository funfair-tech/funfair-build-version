using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests
{
    public sealed class SimpleBranchClassificationTests : TestBase
    {
        private readonly IBranchClassification _branchClassification;

        public SimpleBranchClassificationTests()
        {
            this._branchClassification = new BranchClassification();
        }

        [Theory]
        [InlineData("master")]
        [InlineData("feature/update-branch")]
        [InlineData("depends/update-components")]
        [InlineData("release")]
        [InlineData("release/monkey")]
        [InlineData("release/1.2.3-example")]
        [InlineData("release/1.2.3.4")]
        [InlineData("hotfix")]
        [InlineData("hotfix/not-a-version")]
        [InlineData("refs/pull/77/head")]
        public void ShouldNotBeConsideredRelease(string branchName)
        {
            bool isRelease = this._branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.Null(version);
        }

        [Theory]
        [InlineData("release/1", "1.0.0.0")]
        [InlineData("release/1.0", "1.0.0.0")]
        [InlineData("release/1.0.1", "1.0.1.0")]
        public void ShouldBeConsideredRelease(string branchName, string expectedVersionString)
        {
            bool isRelease = this._branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.True(condition: isRelease, userMessage: "Branch should be considered a release branch");
            Assert.NotNull(version);
            Assert.Equal(expected: expectedVersionString, version!.ToString());
        }

        [Theory]
        [InlineData("refs/pull/77/head", 77)]
        public void ShouldBeConsideredAPullRequest(string branchName, long expectedPullRequestId)
        {
            bool isRelease = this._branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.True(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: expectedPullRequestId, actual: pullRequestId);
        }

        [Theory]
        [InlineData("master")]
        [InlineData("feature/update-branch")]
        [InlineData("depends/update-components")]
        [InlineData("release")]
        [InlineData("release/monkey")]
        [InlineData("release/1.2.3-example")]
        [InlineData("release/1.2.3.4")]
        [InlineData("hotfix")]
        [InlineData("hotfix/not-a-version")]
        [InlineData("release/1")]
        [InlineData("release/1.0")]
        [InlineData("release/1.0.1")]
        public void ShouldNotBeConsideredAPullRequest(string branchName)
        {
            bool isRelease = this._branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: 0, actual: pullRequestId);
        }
    }

    public sealed class GroupBranchClassificationTests : TestBase
    {
        private readonly IBranchClassification _branchClassification;

        public GroupBranchClassificationTests()
        {
            this._branchClassification = new BranchClassification();
        }

        [Theory]
        [InlineData("master")]
        [InlineData("feature/update-branch")]
        [InlineData("depends/update-components")]
        [InlineData("release")]
        [InlineData("release/monkey")]
        [InlineData("release/1.2.3-example")]
        [InlineData("release/1.2.3.4")]
        [InlineData("hotfix")]
        [InlineData("hotfix/not-a-version")]
        [InlineData("refs/pull/77/head")]
        public void ShouldNotBeConsideredRelease(string branchName)
        {
            bool isRelease = this._branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.Null(version);
        }

        [Theory]
        [InlineData("release/1", "1.0.0.0")]
        [InlineData("release/1.0", "1.0.0.0")]
        [InlineData("release/1.0.1", "1.0.1.0")]
        public void ShouldBeConsideredRelease(string branchName, string expectedVersionString)
        {
            bool isRelease = this._branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.True(condition: isRelease, userMessage: "Branch should be considered a release branch");
            Assert.NotNull(version);
            Assert.Equal(expected: expectedVersionString, version!.ToString());
        }

        [Theory]
        [InlineData("refs/pull/77/head", 77)]
        public void ShouldBeConsideredAPullRequest(string branchName, long expectedPullRequestId)
        {
            bool isRelease = this._branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.True(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: expectedPullRequestId, actual: pullRequestId);
        }

        [Theory]
        [InlineData("master")]
        [InlineData("feature/update-branch")]
        [InlineData("depends/update-components")]
        [InlineData("release")]
        [InlineData("release/monkey")]
        [InlineData("release/1.2.3-example")]
        [InlineData("release/1.2.3.4")]
        [InlineData("hotfix")]
        [InlineData("hotfix/not-a-version")]
        [InlineData("release/1")]
        [InlineData("release/1.0")]
        [InlineData("release/1.0.1")]
        public void ShouldNotBeConsideredAPullRequest(string branchName)
        {
            bool isRelease = this._branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: 0, actual: pullRequestId);
        }
    }
}