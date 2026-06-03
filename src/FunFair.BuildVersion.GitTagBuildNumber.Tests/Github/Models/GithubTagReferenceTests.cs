using FunFair.BuildVersion.GitTagBuildNumber.Github.Models;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github.Models;

public sealed class GithubTagReferenceTests : TestBase
{
    [Fact]
    public void ConstructedObjectShouldHaveCorrectProperties()
    {
        const string reference = "refs/tags/build-number-42";
        const string nodeId = "node-id-123";
        const string url = "https://api.github.com/repos/org/repo/git/refs/tags/build-number-42";
        GithubTagObject obj = new(
            sha: "abc123",
            objectType: "commit",
            url: "https://api.github.com/repos/org/repo/git/commits/abc123"
        );

        GithubTagReference tagReference = new(reference: reference, nodeId: nodeId, url: url, obj: obj);

        Assert.Equal(expected: reference, actual: tagReference.Reference);
        Assert.Equal(expected: nodeId, actual: tagReference.NodeId);
        Assert.Equal(expected: url, actual: tagReference.Url);
        Assert.Same(expected: obj, actual: tagReference.Obj);
    }
}
