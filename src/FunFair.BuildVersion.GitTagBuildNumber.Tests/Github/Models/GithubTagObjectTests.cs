using FunFair.BuildVersion.GitTagBuildNumber.Github.Models;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github.Models;

public sealed class GithubTagObjectTests : TestBase
{
    [Fact]
    public void ConstructedObjectShouldHaveCorrectProperties()
    {
        const string sha = "abc123def456";
        const string objectType = "commit";
        const string url = "https://api.github.com/repos/org/repo/git/commits/abc123def456";

        GithubTagObject tagObject = new(sha: sha, objectType: objectType, url: url);

        Assert.Equal(expected: sha, actual: tagObject.Sha);
        Assert.Equal(expected: objectType, actual: tagObject.ObjectType);
        Assert.Equal(expected: url, actual: tagObject.Url);
    }
}
