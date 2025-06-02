using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FunFair.BuildVersion.GitTagBuildNumber.Exceptions;
using FunFair.BuildVersion.GitTagBuildNumber.Github.Models;
using FunFair.BuildVersion.GitTagBuildNumber.Helpers;

namespace FunFair.BuildVersion.GitTagBuildNumber.Github;

public static class BuildTagNumber
{
    private const int MAX_ATTEMPTS = 10;

    private const string MIME_TYPE = "application/vnd.github+json";

    public static async ValueTask<int> GetNextBuildNumberAsync(
        GitHubContext context,
        CancellationToken cancellationToken
    )
    {
        using (HttpClient httpClient = CreateClient(context))
        {
            int currentVersion = await GetCurrentAsync(
                context: context,
                httpClient: httpClient,
                cancellationToken: cancellationToken
            );

            return await SetCurrentVersionAsync(
                context: context,
                nextBuildNumber: ++currentVersion,
                httpClient: httpClient,
                cancellationToken: cancellationToken
            );
        }
    }

    private static async ValueTask<int> GetCurrentAsync(
        GitHubContext context,
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        Uri uri = new($"https://api.github.com/repos/{context.Repository}/git/refs/tags/{context.Prefix}build-number-");

        using (
            HttpResponseMessage result = await httpClient.GetAsync(
                requestUri: uri,
                cancellationToken: cancellationToken
            )
        )
        {
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                Console.WriteLine("No build number found - starting at 0.");

                return 0;
            }

            if (!result.IsSuccessStatusCode)
            {
                return CouldNotRetrieveExistingBuildTags(result.StatusCode);
            }

            await using (Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken))
            {
                IReadOnlyList<GithubTagReference>? items = await JsonSerializer.DeserializeAsync(
                    utf8Json: stream,
                    jsonTypeInfo: GithubApiJsonSerializerContext.Default.IReadOnlyListGithubTagReference,
                    cancellationToken: cancellationToken
                );

                return items is null ? 0 : ExtractVersionFromTags(items);
            }
        }
    }

    [DoesNotReturn]
    private static int CouldNotRetrieveExistingBuildTags(HttpStatusCode statusCode)
    {
        throw new BuildTagNumberException($"Failed to get build tags - invalid http code: {(int)statusCode}");
    }

    private static int ExtractVersionFromTags(IReadOnlyList<GithubTagReference> items)
    {
        if (items is [])
        {
            return 0;
        }

        return items
            .Select(item => GitUrlProtocolRegex.BuildNumbersFromTag().Match(item.Reference))
            .Where(match => match.Success)
            .Select(match => match.Groups["BuildNumber"].Value)
            .Max(versionText =>
                int.TryParse(
                    s: versionText,
                    style: NumberStyles.Integer,
                    provider: CultureInfo.InvariantCulture,
                    out int version
                )
                    ? version
                    : 0
            );
    }

    private static async ValueTask<int> SetCurrentVersionAsync(
        GitHubContext context,
        int nextBuildNumber,
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        Uri uri = new($"https://api.github.com/repos/{context.Repository}/git/refs");

        int attempts = 0;

        do
        {
            using (StringContent stringContent = BuildAddTagModel(context: context, nextBuildNumber: nextBuildNumber))
            {
                using (
                    HttpResponseMessage result = await httpClient.PostAsync(
                        requestUri: uri,
                        content: stringContent,
                        cancellationToken: cancellationToken
                    )
                )
                {
                    if (result.StatusCode == HttpStatusCode.Created)
                    {
                        return nextBuildNumber;
                    }

                    if (result.StatusCode == HttpStatusCode.UnprocessableContent)
                    {
                        Console.WriteLine(
                            $"Retrying updating to build number: {nextBuildNumber} (Attempt {attempts} of {MAX_ATTEMPTS})"
                        );
                        ++attempts;
                        await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                        continue;
                    }

                    if (result.StatusCode != HttpStatusCode.Conflict)
                    {
                        return CouldNotUpdateBuildNumberHttpError(
                            nextBuildNumber: nextBuildNumber,
                            statusCode: result.StatusCode
                        );
                    }

                    Console.WriteLine($"Attempting next build number {nextBuildNumber} - current build number in use.");

                    ++attempts;
                    ++nextBuildNumber;
                }
            }
        } while (attempts < MAX_ATTEMPTS);

        return CouldNotUpdateBuildNumberRetriesExceeded(nextBuildNumber: nextBuildNumber, maxAttempts: MAX_ATTEMPTS);
    }

    private static StringContent BuildAddTagModel(in GitHubContext context, int nextBuildNumber)
    {
        GithubNewTagRef newTagRef = new($"refs/tags/{context.Prefix}build-number-{nextBuildNumber}", sha: context.Sha);

        string content = JsonSerializer.Serialize(
            value: newTagRef,
            jsonTypeInfo: GithubApiJsonSerializerContext.Default.GithubNewTagRef
        );

        return new(content: content, encoding: Encoding.UTF8, new MediaTypeHeaderValue(MIME_TYPE));
    }

    [DoesNotReturn]
    private static int CouldNotUpdateBuildNumberRetriesExceeded(int nextBuildNumber, int maxAttempts)
    {
        throw new BuildTagNumberException(
            $"Failed to update build number {nextBuildNumber} - Too many attempts: {maxAttempts}"
        );
    }

    [DoesNotReturn]
    private static int CouldNotUpdateBuildNumberHttpError(int nextBuildNumber, HttpStatusCode statusCode)
    {
        throw new BuildTagNumberException(
            $"Failed to update build number {nextBuildNumber} - http error: {(int)statusCode}."
        );
    }

    private static HttpClient CreateClient(in GitHubContext context)
    {
        HttpClient client = new()
        {
            BaseAddress = new("https://api.github.com/"),
            DefaultRequestVersion = new(major: 2, minor: 0),
            DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher,
        };
        client.DefaultRequestHeaders.Authorization = new(scheme: "token", parameter: context.Token);
        client.DefaultRequestHeaders.UserAgent.Add(
            new(productName: VersionInformation.Product, productVersion: VersionInformation.Version)
        );
        client.DefaultRequestHeaders.Accept.Add(new(MIME_TYPE));
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        return client;
    }
}
