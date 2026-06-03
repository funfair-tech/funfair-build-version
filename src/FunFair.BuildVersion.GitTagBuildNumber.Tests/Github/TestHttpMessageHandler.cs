using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Github;

internal sealed class TestHttpMessageHandler : HttpMessageHandler
{
    private readonly Queue<(HttpStatusCode StatusCode, string? JsonContent)> _responses = new();

    public void EnqueueStatus(HttpStatusCode statusCode)
    {
        this._responses.Enqueue((statusCode, null));
    }

    public void EnqueueJsonResponse(HttpStatusCode statusCode, string json)
    {
        this._responses.Enqueue((statusCode, json));
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    )
    {
        (HttpStatusCode statusCode, string? json) = this._responses.Dequeue();

        HttpResponseMessage response = new(statusCode);

        if (json is not null)
        {
            response.Content = new StringContent(content: json, encoding: Encoding.UTF8, mediaType: "application/json");
        }

        return Task.FromResult(response);
    }
}
