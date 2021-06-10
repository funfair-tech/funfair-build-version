using System;
using System.Collections.Generic;
using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests
{
    public sealed class VersionDetectorTests : TestBase
    {
        private readonly IBranchClassification _branchClassification;
        private readonly IBranchDiscovery _branchDiscovery;
        private readonly IVersionDetector _versionDetector;

        public VersionDetectorTests()
        {
            this._branchDiscovery = Substitute.For<IBranchDiscovery>();
            this._branchClassification = Substitute.For<IBranchClassification>();

            this._versionDetector = new VersionDetector(branchDiscovery: this._branchDiscovery,
                                                        branchClassification: this._branchClassification,
                                                        Substitute.For<ILogger<VersionDetector>>());
        }

        [Fact]
        public void WhenCurrentlyOnAReleaseBranch()
        {
            const string branchName = "release/1.0.0";
            this.MockFindCurrentBranch(branchName);
            this.MockIsRelease(branchName: branchName, version: "1.0.0.0");

            NuGetVersion version = AssertReallyNotNull(this._versionDetector.FindVersion(27));

            Assert.Equal(expected: "1.0.0.27", version.ToString());

            this.ReceivedFindCurrentBranch();
            this.ReceivedIsRelease(branchName);
            this.DidNotReceiveFindBranch();
        }

        [Theory]
        [InlineData("master", "master")]
        [InlineData("feature/improve-performance", "improve-perform")]
        [InlineData("feature/test/_/_/_/_/test", "test-test")]
        [InlineData("feature/_/_/_/_", "prerelease")]
        public void WhenCurrentlyOnAPreReleaseWithNoReleaseBranchesBranch(string branchName, string expectedPreReleaseSuffix)
        {
            this.MockFindCurrentBranch(branchName);
            this.MockIsRelease(branchName: "release/1.0.0", version: "1.0.0.0");

            NuGetVersion version = AssertReallyNotNull(this._versionDetector.FindVersion(27));

            Assert.Equal("0.0.1.27-" + expectedPreReleaseSuffix, version.ToString());

            this.ReceivedFindCurrentBranch();
            this.ReceivedIsRelease(branchName);
            this.ReceivedFindBranch();
        }

        [Theory]
        [InlineData("master", "master")]
        [InlineData("feature/improve-performance", "improve-perform")]
        [InlineData("feature/test/_/_/_/_/test", "test-test")]
        [InlineData("feature/_/_/_/_", "prerelease")]
        public void WhenCurrentlyOnAPreReleaseWithReleaseBranchesBranch(string branchName, string expectedPreReleaseSuffix)
        {
            this.MockFindCurrentBranch(branchName);

            IReadOnlyList<string> branches = new[] {"release/1.0.0", "release/1.1.0", "release/3.4.5"};
            this.MockFindBranches(branches);
            this.MockIsRelease(branchName: "release/1.0.0", version: "1.0.0.0");
            this.MockIsRelease(branchName: "release/1.1.0", version: "1.1.0.0");
            this.MockIsRelease(branchName: "release/3.4.5", version: "3.4.5.0");

            NuGetVersion version = AssertReallyNotNull(this._versionDetector.FindVersion(27));

            Assert.Equal("3.4.6.27-" + expectedPreReleaseSuffix, version.ToString());

            this.ReceivedFindCurrentBranch();
            this.ReceivedIsRelease(branchName);
            this.ReceivedIsRelease("release/1.0.0");
            this.ReceivedIsRelease("release/1.1.0");
            this.ReceivedIsRelease("release/3.4.5");
            this.ReceivedFindBranch();
        }

        [Theory]
        [InlineData("refs/pulls/42/head", 42, "pr-42")]
        [InlineData("refs/pulls/474/head", 474, "pr-474")]
        public void WhenCurrentlyOnAPullRequestWithNoReleaseBranchesBranch(string branchName, int pullRequestId, string expectedPreReleaseSuffix)
        {
            this.MockFindCurrentBranch(branchName);
            this.MockIsRelease(branchName: "release/1.0.0", version: "1.0.0.0");

            this.MockIsPullRequest(branchName: branchName, id: pullRequestId);

            NuGetVersion version = AssertReallyNotNull(this._versionDetector.FindVersion(27));

            Assert.Equal("0.0.1.27-" + expectedPreReleaseSuffix, version.ToString());

            this.ReceivedFindCurrentBranch();
            this.ReceivedIsRelease(branchName);
            this.ReceivedFindBranch();
        }

        [Theory]
        [InlineData("refs/pulls/42/head", 42, "pr-42")]
        [InlineData("refs/pulls/714/head", 714, "pr-714")]
        public void WhenCurrentlyOnAPullRequestWithReleaseBranchesBranch(string branchName, int pullRequestId, string expectedPreReleaseSuffix)
        {
            this.MockFindCurrentBranch(branchName);

            IReadOnlyList<string> branches = new[] {"release/1.0.0", "release/1.1.0", "release/3.4.5"};
            this.MockFindBranches(branches);
            this.MockIsRelease(branchName: "release/1.0.0", version: "1.0.0.0");
            this.MockIsRelease(branchName: "release/1.1.0", version: "1.1.0.0");
            this.MockIsRelease(branchName: "release/3.4.5", version: "3.4.5.0");

            this.MockIsPullRequest(branchName: branchName, id: pullRequestId);

            NuGetVersion version = AssertReallyNotNull(this._versionDetector.FindVersion(27));

            Assert.Equal("3.4.6.27-" + expectedPreReleaseSuffix, version.ToString());

            this.ReceivedFindCurrentBranch();
            this.ReceivedIsRelease(branchName);
            this.ReceivedIsRelease("release/1.0.0");
            this.ReceivedIsRelease("release/1.1.0");
            this.ReceivedIsRelease("release/3.4.5");
            this.ReceivedFindBranch();
        }

        private void MockFindBranches(IReadOnlyList<string> branches)
        {
            this._branchDiscovery.FindBranches()
                .Returns(branches);
        }

        private void ReceivedIsRelease(string branchName)
        {
            this._branchClassification.Received(1)
                .IsRelease(branchName: branchName, out Arg.Any<NuGetVersion?>());
        }

        private void MockIsRelease(string branchName, string version)
        {
            this._branchClassification.IsRelease(branchName: branchName, out Arg.Any<NuGetVersion?>())
                .Returns(x =>
                         {
                             x[1] = new NuGetVersion(new Version(version));

                             return true;
                         });
        }

        private void DidNotReceiveFindBranch()
        {
            this._branchDiscovery.DidNotReceive()
                .FindBranches();
        }

        private void ReceivedFindBranch()
        {
            this._branchDiscovery.Received(1)
                .FindBranches();
        }

        private void ReceivedFindCurrentBranch()
        {
            this._branchDiscovery.Received(1)
                .FindCurrentBranch();
        }

        private void MockFindCurrentBranch(string branchName)
        {
            this._branchDiscovery.FindCurrentBranch()
                .Returns(branchName);
        }

        private void MockIsPullRequest(string branchName, long id)
        {
            this._branchClassification.IsPullRequest(currentBranch: branchName, out Arg.Any<long>())
                .Returns(x =>
                         {
                             x[1] = id;

                             return true;
                         });
        }
    }
}