using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using NuGet.Versioning;
using Xunit;

namespace FunFair.BuildVersion.Detection.Tests
{
    public sealed class SimpleBranchClassificationTests : TestBase
    {
        private readonly IBranchClassification _branchClassifcation;

        public SimpleBranchClassificationTests()
        {
            this._branchClassifcation = new BranchClassification();
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
        public void ShouldNotBeConsideredRelease(string branchName)
        {
            bool isRelease = this._branchClassifcation.IsReleaseBranch(branchName: branchName, out NuGetVersion? version);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.Null(version);
        }

        [Theory]
        [InlineData("release/1", "1.0.0.0")]
        [InlineData("release/1.0", "1.0.0.0")]
        [InlineData("release/1.0.1", "1.0.1.0")]
        public void ShouldBeConsideredRelease(string branchName, string expectedVersionString)
        {
            bool isRelease = this._branchClassifcation.IsReleaseBranch(branchName: branchName, out NuGetVersion? version);
            Assert.True(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.NotNull(version);
            Assert.Equal(expected: expectedVersionString, version!.ToString());
        }
    }
}