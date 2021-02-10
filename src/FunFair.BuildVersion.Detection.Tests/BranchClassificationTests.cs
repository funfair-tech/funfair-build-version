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
        public void ShouldNotBeConsideredRelease(string branchName)
        {
            bool isRelease = this._branchClassifcation.IsReleaseBranch(branchName: branchName, out NuGetVersion? version);
            Assert.False(condition: isRelease, userMessage: "Branch should not be considered a release branch");
            Assert.Null(version);
        }
    }
}