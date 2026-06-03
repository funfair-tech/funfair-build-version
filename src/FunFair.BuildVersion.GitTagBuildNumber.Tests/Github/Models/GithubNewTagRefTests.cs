using FunFair.BuildVersion.GitTagBuildNumber.Github.Models;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github.Models;

public sealed class GithubNewTagRefTests : TestBase
{
    [Fact]
    public void ConstructedObjectShouldHaveCorrectProperties()
    {
        const string reference = "refs/tags/v1.0.0-42";
        const string sha = "abc123def456";

        GithubNewTagRef tagRef = new(reference: reference, sha: sha);

        Assert.Equal(expected: reference, actual: tagRef.Reference);
        Assert.Equal(expected: sha, actual: tagRef.Sha);
    }
}
