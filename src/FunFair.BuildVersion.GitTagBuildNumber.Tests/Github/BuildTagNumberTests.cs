using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FunFair.BuildVersion.GitTagBuildNumber.Exceptions;
using FunFair.BuildVersion.GitTagBuildNumber.Github;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github;

public sealed class BuildTagNumberTests : TestBase
{
    private static readonly GitHubContext TestContext = new(
        Token: "test-token",
        Repository: "org/repo",
        Sha: "abc123sha",
        Prefix: "v"
    );

    [Theory]
    [InlineData(HttpStatusCode.NotFound, null)]
    [InlineData(HttpStatusCode.OK, "null")]
    [InlineData(HttpStatusCode.OK, "[]")]
    public async ValueTask WhenGetReturnsNoTagsShouldStartAtOne(HttpStatusCode getStatusCode, string? getJson)
    {
        using TestHttpMessageHandler handler = new();

        if (getJson is null)
        {
            handler.EnqueueStatus(getStatusCode);
        }
        else
        {
            handler.EnqueueJsonResponse(getStatusCode, getJson);
        }

        handler.EnqueueStatus(HttpStatusCode.Created);

        int buildNumber = await BuildTagNumber.GetNextBuildNumberAsync(
            context: TestContext,
            messageHandler: handler,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 1, actual: buildNumber);
    }

    [Fact]
    public async ValueTask WhenTagsExistShouldReturnMaxPlusOne()
    {
        const string json =
            "[{\"ref\":\"refs/tags/vbuild-number-5\",\"node_id\":\"node1\",\"url\":\"https://api.github.com/repos/org/repo/git/refs/tags/vbuild-number-5\",\"object\":{\"sha\":\"abc\",\"type\":\"commit\",\"url\":\"https://api.github.com/repos/org/repo/git/commits/abc\"}},{\"ref\":\"refs/tags/vbuild-number-10\",\"node_id\":\"node2\",\"url\":\"https://api.github.com/repos/org/repo/git/refs/tags/vbuild-number-10\",\"object\":{\"sha\":\"def\",\"type\":\"commit\",\"url\":\"https://api.github.com/repos/org/repo/git/commits/def\"}}]";

        using TestHttpMessageHandler handler = new();
        handler.EnqueueJsonResponse(HttpStatusCode.OK, json);
        handler.EnqueueStatus(HttpStatusCode.Created);

        int buildNumber = await BuildTagNumber.GetNextBuildNumberAsync(
            context: TestContext,
            messageHandler: handler,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 11, actual: buildNumber);
    }

    [Fact]
    public async ValueTask WhenGetReturnsNonSuccessNonNotFoundShouldThrow()
    {
        using TestHttpMessageHandler handler = new();
        handler.EnqueueStatus(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<BuildTagNumberException>(() =>
            BuildTagNumber
                .GetNextBuildNumberAsync(
                    context: TestContext,
                    messageHandler: handler,
                    cancellationToken: this.CancellationToken()
                )
                .AsTask()
        );
    }

    [Fact]
    public async ValueTask WhenPostReturnsConflictShouldIncrementAndRetry()
    {
        const string json = "[]";

        using TestHttpMessageHandler handler = new();
        handler.EnqueueJsonResponse(HttpStatusCode.OK, json);
        handler.EnqueueStatus(HttpStatusCode.Conflict);
        handler.EnqueueStatus(HttpStatusCode.Created);

        int buildNumber = await BuildTagNumber.GetNextBuildNumberAsync(
            context: TestContext,
            messageHandler: handler,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 2, actual: buildNumber);
    }

    [Fact]
    public async ValueTask WhenPostReturnsOtherErrorShouldThrow()
    {
        const string json = "[]";

        using TestHttpMessageHandler handler = new();
        handler.EnqueueJsonResponse(HttpStatusCode.OK, json);
        handler.EnqueueStatus(HttpStatusCode.InternalServerError);

        await Assert.ThrowsAsync<BuildTagNumberException>(() =>
            BuildTagNumber
                .GetNextBuildNumberAsync(
                    context: TestContext,
                    messageHandler: handler,
                    cancellationToken: this.CancellationToken()
                )
                .AsTask()
        );
    }

    [Theory]
    [InlineData(HttpStatusCode.Conflict)]
    [InlineData(HttpStatusCode.UnprocessableContent)]
    public async ValueTask WhenPostExceedsMaxAttemptsShouldThrow(HttpStatusCode retryStatusCode)
    {
        const string json = "[]";

        using TestHttpMessageHandler handler = new();
        handler.EnqueueJsonResponse(HttpStatusCode.OK, json);

        for (int i = 0; i < 10; i++)
        {
            handler.EnqueueStatus(retryStatusCode);
        }

        BuildTagNumberException exception = await Assert.ThrowsAsync<BuildTagNumberException>(() =>
            BuildTagNumber
                .GetNextBuildNumberAsync(
                    context: TestContext,
                    messageHandler: handler,
                    cancellationToken: this.CancellationToken()
                )
                .AsTask()
        );

        Assert.Contains(
            expectedSubstring: "Too many attempts",
            actualString: exception.Message,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async ValueTask WhenPostReturnsUnprocessableContentShouldRetryAndEventuallySucceed()
    {
        const string json = "[]";

        using TestHttpMessageHandler handler = new();
        handler.EnqueueJsonResponse(HttpStatusCode.OK, json);
        handler.EnqueueStatus(HttpStatusCode.UnprocessableContent);
        handler.EnqueueStatus(HttpStatusCode.Created);

        int buildNumber = await BuildTagNumber.GetNextBuildNumberAsync(
            context: TestContext,
            messageHandler: handler,
            cancellationToken: this.CancellationToken()
        );

        Assert.Equal(expected: 1, actual: buildNumber);
    }

    [Fact]
    public async ValueTask WhenCancellationTokenIsAlreadyCancelledShouldThrowOperationCancelledException()
    {
        using CancellationTokenSource cts = new();
        await cts.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            BuildTagNumber.GetNextBuildNumberAsync(context: TestContext, cancellationToken: cts.Token).AsTask()
        );
    }
}
