using FunFair.BuildVersion.GitTagBuildNumber.Github;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests;

public sealed class RepoUrlParserTests : TestBase
{
    [Theory]
    [InlineData("https://github.com/org/repo.git", "github.com", "org/repo")]
    [InlineData("https://github.com/org/repo", "github.com", "org/repo")]
    [InlineData("https://github.com/org/sub/repo.git", "github.com", "org/sub/repo")]
    [InlineData("http://github.com/org/repo.git", "github.com", "org/repo")]
    [InlineData("http://github.com/org/repo", "github.com", "org/repo")]
    public void HttpsUrlShouldReturnHttpProtocol(string url, string expectedHost, string expectedRepo)
    {
        bool result = RepoUrlParser.TryParse(
            path: url,
            protocol: out GitUrlProtocol protocol,
            host: out string? host,
            repo: out string? repo
        );

        Assert.True(condition: result, userMessage: "Expected TryParse to return true for HTTP URL");
        Assert.Equal(expected: GitUrlProtocol.HTTP, actual: protocol);
        Assert.Equal(expected: expectedHost, actual: host);
        Assert.Equal(expected: expectedRepo, actual: repo);
    }

    [Theory]
    [InlineData("git@github.com:org/repo.git", "github.com", "org/repo")]
    [InlineData("git@github.com:org/repo", "github.com", "org/repo")]
    [InlineData("git@bitbucket.org:user/project.git", "bitbucket.org", "user/project")]
    [InlineData("git@gitlab.com:group/subgroup/repo.git", "gitlab.com", "group/subgroup/repo")]
    public void SshUrlShouldReturnSshProtocol(string url, string expectedHost, string expectedRepo)
    {
        bool result = RepoUrlParser.TryParse(
            path: url,
            protocol: out GitUrlProtocol protocol,
            host: out string? host,
            repo: out string? repo
        );

        Assert.True(condition: result, userMessage: "Expected TryParse to return true for SSH URL");
        Assert.Equal(expected: GitUrlProtocol.SSH, actual: protocol);
        Assert.Equal(expected: expectedHost, actual: host);
        Assert.Equal(expected: expectedRepo, actual: repo);
    }

    [Theory]
    [InlineData("ftp://example.com/repo")]
    [InlineData("ssh://example.com/repo")]
    [InlineData("file:///some/path")]
    public void NonHttpAbsoluteUriShouldReturnFalse(string url)
    {
        bool result = RepoUrlParser.TryParse(
            path: url,
            protocol: out GitUrlProtocol protocol,
            host: out string? host,
            repo: out string? repo
        );

        Assert.False(condition: result, userMessage: "Expected TryParse to return false for non-HTTP absolute URI");
        Assert.Equal(expected: GitUrlProtocol.UNKNOWN, actual: protocol);
        Assert.Null(host);
        Assert.Null(repo);
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("just some text")]
    [InlineData("")]
    public void UnrecognisedStringShouldReturnFalse(string input)
    {
        bool result = RepoUrlParser.TryParse(
            path: input,
            protocol: out GitUrlProtocol protocol,
            host: out string? host,
            repo: out string? repo
        );

        Assert.False(condition: result, userMessage: "Expected TryParse to return false for unrecognised input");
        Assert.Equal(expected: GitUrlProtocol.UNKNOWN, actual: protocol);
        Assert.Null(host);
        Assert.Null(repo);
    }
}
