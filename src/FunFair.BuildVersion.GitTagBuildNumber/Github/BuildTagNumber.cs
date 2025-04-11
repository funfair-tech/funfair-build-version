using System;
using System.Collections.Generic;
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
using FunFair.BuildVersion.GitTagBuildNumber.Github.Models;
using FunFair.BuildVersion.GitTagBuildNumber.Helpers;

namespace FunFair.BuildVersion.GitTagBuildNumber.Github;

public static class BuildTagNumber
{
    public static async ValueTask<int> UpdateBuildNumberTagAsync(
        GitHubContext context,
        CancellationToken cancellationToken
    )
    {
        using (HttpClient httpClient = CreateClient(context))
        {
            int currentVersion = await GetCurrentVersionAsync(
                context: context,
                httpClient: httpClient,
                cancellationToken: cancellationToken
            );
            ++currentVersion;

            await UpdateCurrentVersionAsync(
                context: context,
                nextBuildNumber: currentVersion,
                httpClient: httpClient,
                cancellationToken: cancellationToken
            );

            return currentVersion;
        }
    }

    private static async ValueTask<int> GetCurrentVersionAsync(
        GitHubContext context,
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        Uri uri = new(
            $"https://api.github.com/repos/{context.Repository}/git/refs/tags/{context.Prefix}build-number-"
        );

        using (
            HttpResponseMessage result = await httpClient.GetAsync(
                requestUri: uri,
                cancellationToken: cancellationToken
            )
        )
        {
            if (result.StatusCode == HttpStatusCode.NotFound)
            {
                return 0;
            }

            if (!result.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Failed to get build tags");
            }

            await using (Stream stream = await result.Content.ReadAsStreamAsync(cancellationToken))
            {
                IReadOnlyList<GithubTagReference>? items = await JsonSerializer.DeserializeAsync<
                    IReadOnlyList<GithubTagReference>
                >(
                    utf8Json: stream,
                    jsonTypeInfo: GithubApiJsonSerializerContext
                        .Default
                        .IReadOnlyListGithubTagReference,
                    cancellationToken: cancellationToken
                );

                return items is null ? 0 : ExtractVersionFromTags(items);
            }
        }
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

    private static async ValueTask UpdateCurrentVersionAsync(
        GitHubContext context,
        int nextBuildNumber,
        HttpClient httpClient,
        CancellationToken cancellationToken
    )
    {
        int attempts = 0;

        do
        {
            Uri uri = new($"https://api.github.com/repos/{context.Repository}/git/refs");
            GithubNewTagRef newTagRef = new(
                $"refs/tags/{context.Prefix}build-number-{nextBuildNumber}",
                sha: context.Sha
            );

            string content = JsonSerializer.Serialize<GithubNewTagRef>(
                value: newTagRef,
                jsonTypeInfo: GithubApiJsonSerializerContext.Default.GithubNewTagRef
            );

            using (
                StringContent stringContent = new(
                    content: content,
                    encoding: Encoding.UTF8,
                    new MediaTypeHeaderValue("application/json")
                )
            )
            {
                using (
                    HttpResponseMessage result = await httpClient.PostAsync(
                        requestUri: uri,
                        content: stringContent,
                        cancellationToken: cancellationToken
                    )
                )
                {
                    if (!result.IsSuccessStatusCode)
                    {
                        throw new HttpRequestException(
                            $"Failed to update build number {nextBuildNumber}."
                        );
                    }

                    if (result.StatusCode == HttpStatusCode.Created)
                    {
                        return;
                    }

                    Console.WriteLine($"Attempting next build number {nextBuildNumber}.");

                    ++attempts;
                    ++nextBuildNumber;
                }
            }
        } while (attempts < 10);
    }

    private static HttpClient CreateClient(in GitHubContext context)
    {
        HttpClient client = new() { BaseAddress = new("https://api.github.com/") };
        client.DefaultRequestHeaders.Authorization = new(scheme: "token", parameter: context.Token);
        client.DefaultRequestHeaders.UserAgent.Add(
            new(productName: VersionInformation.Product, productVersion: VersionInformation.Version)
        );

        return client;
    }
}
