using System.Collections.Generic;
using FunFair.BuildVersion.Interfaces;
using FunFair.Test.Common;
using LibGit2Sharp;
using NSubstitute;
using NuGet.Versioning;
using Xunit;
using Version = System.Version;

namespace FunFair.BuildVersion.Detection.Tests;

public sealed class VersionDetectorWithRealLoggerTests : LoggingFolderCleanupTestBase
{
    private readonly IBranchClassification _branchClassification;
    private readonly IBranchDiscovery _branchDiscovery;
    private readonly Repository _repository;
    private readonly IVersionDetector _versionDetector;

    public VersionDetectorWithRealLoggerTests(ITestOutputHelper output)
        : base(output)
    {
        this._branchDiscovery = GetSubstitute<IBranchDiscovery>();
        this._branchClassification = GetSubstitute<IBranchClassification>();

        Repository.Init(this.TempFolder);
        this._repository = new(this.TempFolder);

        this._versionDetector = new VersionDetector(
            branchDiscovery: this._branchDiscovery,
            branchClassification: this._branchClassification,
            logger: this.GetTypedLogger<VersionDetector>()
        );
    }

    [Fact]
    public void WhenOnReleaseBranch_WithRealLogger_ReturnsVersionWithBuildNumber()
    {
        const string branchName = "release/2.0.0";
        this._branchDiscovery.FindCurrentBranch(Arg.Any<Repository>()).Returns(branchName);
        this._branchClassification.IsRelease(branchName: branchName, out Arg.Any<NuGetVersion?>())
            .Returns(x =>
            {
                x[1] = new NuGetVersion(new Version("2.0.0.0"));

                return true;
            });

        NuGetVersion version = AssertReallyNotNull(
            this._versionDetector.FindVersion(repository: this._repository, buildNumber: 5)
        );

        Assert.Equal(expected: "2.0.0.5", actual: version.ToString());
    }

    [Fact]
    public void WhenOnPreReleaseBranchWithReleaseBranches_WithRealLogger_ReturnsPreReleaseVersion()
    {
        const string branchName = "feature/new-feature";
        IReadOnlyList<string> branches = ["release/1.0.0", "release/2.0.0"];
        this._branchDiscovery.FindCurrentBranch(Arg.Any<Repository>()).Returns(branchName);
        this._branchDiscovery.FindBranches(Arg.Any<Repository>()).Returns(branches);
        this._branchClassification.IsRelease(branchName: branchName, out Arg.Any<NuGetVersion?>()).Returns(false);
        this._branchClassification.IsRelease(branchName: "release/1.0.0", out Arg.Any<NuGetVersion?>())
            .Returns(x =>
            {
                x[1] = new NuGetVersion(new Version("1.0.0.0"));

                return true;
            });
        this._branchClassification.IsRelease(branchName: "release/2.0.0", out Arg.Any<NuGetVersion?>())
            .Returns(x =>
            {
                x[1] = new NuGetVersion(new Version("2.0.0.0"));

                return true;
            });

        NuGetVersion version = AssertReallyNotNull(
            this._versionDetector.FindVersion(repository: this._repository, buildNumber: 7)
        );

        Assert.Equal(expected: "2.0.1.7-new-feature", actual: version.ToString());
    }

    [Fact]
    public void WhenOnPreReleaseBranchWithMixedBranches_NonReleaseBranchInList_IsIgnored()
    {
        const string branchName = "feature/another-feature";
        IReadOnlyList<string> branches = ["release/3.0.0", "non-release-branch"];
        this._branchDiscovery.FindCurrentBranch(Arg.Any<Repository>()).Returns(branchName);
        this._branchDiscovery.FindBranches(Arg.Any<Repository>()).Returns(branches);
        this._branchClassification.IsRelease(branchName: branchName, out Arg.Any<NuGetVersion?>()).Returns(false);
        this._branchClassification.IsRelease(branchName: "release/3.0.0", out Arg.Any<NuGetVersion?>())
            .Returns(x =>
            {
                x[1] = new NuGetVersion(new Version("3.0.0.0"));

                return true;
            });
        this._branchClassification.IsRelease(branchName: "non-release-branch", out Arg.Any<NuGetVersion?>())
            .Returns(false);

        NuGetVersion version = AssertReallyNotNull(
            this._versionDetector.FindVersion(repository: this._repository, buildNumber: 3)
        );

        Assert.Equal(expected: "3.0.1.3-another-feature", actual: version.ToString());
    }
}
