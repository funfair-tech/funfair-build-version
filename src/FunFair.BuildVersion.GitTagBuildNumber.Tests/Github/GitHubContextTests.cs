using FunFair.BuildVersion.GitTagBuildNumber.Github;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github;

public sealed class GitHubContextTests : TestBase
{
    [Fact]
    public void ConstructedContextShouldHaveCorrectProperties()
    {
        const string token = "test-token";
        const string repository = "org/repo";
        const string sha = "abc123";
        const string prefix = "v";

        GitHubContext context = new(Token: token, Repository: repository, Sha: sha, Prefix: prefix);

        Assert.Equal(expected: token, actual: context.Token);
        Assert.Equal(expected: repository, actual: context.Repository);
        Assert.Equal(expected: sha, actual: context.Sha);
        Assert.Equal(expected: prefix, actual: context.Prefix);
    }

    [Fact]
    public void EqualContextsShouldBeEqual()
    {
        GitHubContext a = new(Token: "token", Repository: "org/repo", Sha: "sha1", Prefix: "v");
        GitHubContext b = new(Token: "token", Repository: "org/repo", Sha: "sha1", Prefix: "v");

        Assert.Equal(expected: a, actual: b);
    }

    [Fact]
    public void DifferentContextsShouldNotBeEqual()
    {
        GitHubContext a = new(Token: "token-a", Repository: "org/repo", Sha: "sha1", Prefix: "v");
        GitHubContext b = new(Token: "token-b", Repository: "org/repo", Sha: "sha1", Prefix: "v");

        Assert.NotEqual(a, b);
    }
}
