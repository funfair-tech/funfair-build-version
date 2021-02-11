using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests
{
    public sealed class BranchClassificationTests : TestBase
    {
        [Theory]
        [InlineData("", "", "master")]
        [InlineData("", "", "feature/update-branch")]
        [InlineData("", "", "depends/update-components")]
        [InlineData("", "", "release")]
        [InlineData("", "", "release/monkey")]
        [InlineData("", "", "release/1.2.3-example")]
        [InlineData("", "", "release/1.2.3.4")]
        [InlineData("", "", "hotfix")]
        [InlineData("", "", "hotfix/not-a-version")]
        [InlineData("", "", "refs/pull/77/head")]
        [InlineData("", "", "release-test/1.0.1")]
        [InlineData("", "", "release-test/package/1.0.1")]
        public void ShouldNotBeConsideredRelease(string suffix, string package, string branchName)
        {
            IBranchClassification branchClassification = Configure(suffix: suffix, package: package);

            bool isRelease = branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.Null(version);
        }

        private static BranchClassification Configure(string suffix, string package)
        {
            return new(new BranchSettings(releaseSuffix: suffix, package: package));
        }

        [Theory]
        [InlineData("", "", "release/1", "1.0.0.0")]
        [InlineData("", "", "release/1.0", "1.0.0.0")]
        [InlineData("", "", "release/1.0.1", "1.0.1.0")]
        [InlineData("", "banana", "release/banana/2", "2.0.0.0")]
        [InlineData("", "banana", "release/banana/3.7", "3.7.0.0")]
        [InlineData("", "banana", "release/banana/4.2.1", "4.2.1.0")]
        [InlineData("test", "", "release-test/1", "1.0.0.0")]
        [InlineData("test", "", "release-test/1.0", "1.0.0.0")]
        [InlineData("test", "", "release-test/1.0.1", "1.0.1.0")]
        [InlineData("test", "banana", "release-test/banana/2", "2.0.0.0")]
        [InlineData("test", "banana", "release-test/banana/3.7", "3.7.0.0")]
        [InlineData("test", "banana", "release-test/banana/4.2.1", "4.2.1.0")]
        [InlineData("", "", "hotfix/1", "1.0.0.0")]
        [InlineData("", "", "hotfix/1.0", "1.0.0.0")]
        [InlineData("", "", "hotfix/1.0.1", "1.0.1.0")]
        [InlineData("", "banana", "hotfix/banana/2", "2.0.0.0")]
        [InlineData("", "banana", "hotfix/banana/3.7", "3.7.0.0")]
        [InlineData("", "banana", "hotfix/banana/4.2.1", "4.2.1.0")]
        [InlineData("test", "", "hotfix-test/1", "1.0.0.0")]
        [InlineData("test", "", "hotfix-test/1.0", "1.0.0.0")]
        [InlineData("test", "", "hotfix-test/1.0.1", "1.0.1.0")]
        [InlineData("test", "banana", "hotfix-test/banana/2", "2.0.0.0")]
        [InlineData("test", "banana", "hotfix-test/banana/3.7", "3.7.0.0")]
        [InlineData("test", "banana", "hotfix-test/banana/4.2.1", "4.2.1.0")]
        public void ShouldBeConsideredRelease(string suffix, string package, string branchName, string expectedVersionString)
        {
            IBranchClassification branchClassification = Configure(suffix: suffix, package: package);

            bool isRelease = branchClassification.IsRelease(branchName: branchName, out NuGetVersion? version);
            Assert.True(condition: isRelease, userMessage: "Branch should be considered a release branch");
            Assert.NotNull(version);
            Assert.Equal(expected: expectedVersionString, version!.ToString());
        }

        [Theory]
        [InlineData("", "", "refs/pull/77/head", 77)]
        [InlineData("", "", "refs/pull/312/head", 312)]
        [InlineData("test", "", "refs/pull/77/head", 77)]
        [InlineData("", "banana", "refs/pull/77/head", 77)]
        [InlineData("test", "banana", "refs/pull/77/head", 77)]
        public void ShouldBeConsideredAPullRequest(string suffix, string package, string branchName, long expectedPullRequestId)
        {
            IBranchClassification branchClassification = Configure(suffix: suffix, package: package);

            bool isRelease = branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.True(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: expectedPullRequestId, actual: pullRequestId);
        }

        [Theory]
        [InlineData("", "", "master")]
        [InlineData("", "", "feature/update-branch")]
        [InlineData("", "", "depends/update-components")]
        [InlineData("", "", "release")]
        [InlineData("", "", "release/monkey")]
        [InlineData("", "", "release/1.2.3-example")]
        [InlineData("", "", "release/1.2.3.4")]
        [InlineData("", "", "hotfix")]
        [InlineData("", "", "hotfix/not-a-version")]
        [InlineData("", "", "release/1")]
        [InlineData("", "", "release/1.0")]
        [InlineData("", "", "release/1.0.1")]
        public void ShouldNotBeConsideredAPullRequest(string suffix, string package, string branchName)
        {
            IBranchClassification branchClassification = Configure(suffix: suffix, package: package);

            bool isRelease = branchClassification.IsPullRequest(currentBranch: branchName, out long pullRequestId);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a pull request branch");
            Assert.Equal(expected: 0, actual: pullRequestId);
        }
    }
}