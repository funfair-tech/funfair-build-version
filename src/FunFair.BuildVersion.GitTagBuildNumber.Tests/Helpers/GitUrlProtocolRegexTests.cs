using System.Text.RegularExpressions;
using FunFair.BuildVersion.GitTagBuildNumber.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Helpers;

public sealed class GitUrlProtocolRegexTests : TestBase
{
    [Theory]
    [InlineData("git@github.com:org/repo.git", "github.com", "org/repo")]
    [InlineData("git@github.com:org/repo", "github.com", "org/repo")]
    [InlineData("git@bitbucket.org:user/project.git", "bitbucket.org", "user/project")]
    [InlineData("anything@somehost:some/repo.git", "somehost", "some/repo")]
    public void SshHostAndRepoShouldMatchValidSshUrls(string input, string expectedHost, string expectedRepo)
    {
        Regex regex = GitUrlProtocolRegex.SshHostAndRepo();

        Match match = regex.Match(input);

        Assert.True(condition: match.Success, userMessage: "Expected SSH regex to match input");
        Assert.Equal(expected: expectedHost, actual: match.Groups["Host"].Value);
        Assert.Equal(expected: expectedRepo, actual: match.Groups["Repo"].Value);
    }

    [Theory]
    [InlineData("https://github.com/org/repo")]
    [InlineData("not-a-url")]
    [InlineData("")]
    public void SshHostAndRepoShouldNotMatchInvalidInput(string input)
    {
        Regex regex = GitUrlProtocolRegex.SshHostAndRepo();

        Match match = regex.Match(input);

        Assert.False(condition: match.Success, userMessage: "Expected SSH regex to not match invalid input");
    }

    [Theory]
    [InlineData("refs/tags/build-number-42", "42")]
    [InlineData("refs/tags/prefix-build-number-100", "100")]
    [InlineData("refs/tags/v1.0.0-build-number-7", "7")]
    public void BuildNumbersFromTagShouldExtractBuildNumber(string input, string expectedBuildNumber)
    {
        Regex regex = GitUrlProtocolRegex.BuildNumbersFromTag();

        Match match = regex.Match(input);

        Assert.True(condition: match.Success, userMessage: "Expected build number regex to match input");
        Assert.Equal(expected: expectedBuildNumber, actual: match.Groups["BuildNumber"].Value);
    }

    [Theory]
    [InlineData("refs/tags/v1.0.0")]
    [InlineData("no-trailing-number")]
    public void BuildNumbersFromTagShouldNotMatchTagsWithoutBuildNumber(string input)
    {
        Regex regex = GitUrlProtocolRegex.BuildNumbersFromTag();

        Match match = regex.Match(input);

        Assert.False(
            condition: match.Success,
            userMessage: "Expected build number regex to not match tag without build number"
        );
    }
}
