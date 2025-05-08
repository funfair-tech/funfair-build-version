using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using CommandLine;
using FunFair.BuildVersion.Detection;
using FunFair.BuildVersion.GitTagBuildNumber.Github;
using FunFair.BuildVersion.Interfaces;
using FunFair.BuildVersion.Publishers;
using FunFair.BuildVersion.Services;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion;

internal static class Program
{
    private const int SUCCESS = 0;
    private const int ERROR = 1;

    private static Task<int> NotParsedAsync(IEnumerable<Error> errors)
    {
        Console.WriteLine("Errors:");

        foreach (Error error in errors)
        {
            Console.WriteLine($" * {error.Tag.GetName()}");
        }

        return Task.FromResult(ERROR);
    }

    private static async Task<int> ParsedOkAsync(Options options)
    {
        string workDir = Environment.CurrentDirectory;

        using (Repository repository = OpenRepository(workDir))
        {
            int buildNumber = await GetBuildNumberAsync(repository: repository, options: options, cancellationToken: CancellationToken.None);

            IServiceProvider serviceProvider = Setup(options: options);

            IDiagnosticLogger logging = serviceProvider.GetRequiredService<IDiagnosticLogger>();
            IVersionDetector versionDetector = serviceProvider.GetRequiredService<IVersionDetector>();

            NuGetVersion version = versionDetector.FindVersion(repository: repository, buildNumber: buildNumber);

            ApplyVersion(version: version, serviceProvider: serviceProvider);

            if (logging.IsErrored)
            {
                Console.WriteLine();
                Console.WriteLine(logging.Errors > 1
                                      ? $"Found {logging.Errors} Errors"
                                      : $"Found {logging.Errors} Error");

                return ERROR;
            }

            return SUCCESS;
        }
    }

    private static async ValueTask<int> GetBuildNumberAsync(Repository repository, Options options, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(options.GithubToken))
        {
            Remote? remote = repository.Network.Remotes["origin"];

            if (remote is not null)
            {
                if (RepoUrlParser.TryParse(path: remote.Url, out GitUrlProtocol _, out string? host, out string? repo) && StringComparer.OrdinalIgnoreCase.Equals(x: host, y: "github.com"))
                {
                    string prefix = string.IsNullOrWhiteSpace(options.GitTagPrefix)
                        ? ""
                        : options.GitTagPrefix.Trim()
                                 .TrimEnd('-') + "-";
                    GitHubContext context = new(Token: options.GithubToken, Repository: repo, Sha: repository.Head.Tip.Sha, Prefix: prefix);

                    int buildNumber = await BuildTagNumber.GetNextBuildNumberAsync(context: context, cancellationToken: cancellationToken);

                    return buildNumber;
                }
            }
        }

        return FindBuildNumber(options.BuildNumber);
    }

    public static async Task<int> Main(params string[] args)
    {
        try
        {
            Console.WriteLine($"{VersionInformation.Product} {VersionInformation.Version}");

            return await Parser.Default.ParseArguments<Options>(args)
                               .MapResult(parsedFunc: ParsedOkAsync, notParsedFunc: NotParsedAsync);
        }
        catch (Exception exception)
        {
            Console.WriteLine($"ERROR: {exception.Message}");
            Console.WriteLine($"ERROR: {exception.GetType().FullName}");
            Console.WriteLine($"ERROR: {exception.StackTrace}");

            Exception? inner = exception.InnerException;

            if (inner is not null)
            {
                Console.WriteLine($"ERROR: {inner.Message}");
                Console.WriteLine($"ERROR: {inner.GetType().FullName}");
                Console.WriteLine($"ERROR: {inner.StackTrace}");
            }

            return ERROR;
        }
    }

    private static IServiceProvider Setup(Options options)
    {
        DiagnosticLogger logger = new(options.WarningsAsErrors);

        IBranchSettings branchSettings = new BranchSettings(releaseSuffix: options.ReleaseSuffix, package: options.Package);

        return new ServiceCollection().AddSingleton<ILogger>(logger)
                                      .AddSingleton<IDiagnosticLogger>(logger)
                                      .AddSingleton(typeof(ILogger<>), typeof(LoggerProxy<>))
                                      .AddBuildVersionDetection(branchSettings: branchSettings)
                                      .AddSingleton<IVersionPublisher, GitHubActionsVersionPublisher>()
                                      .AddSingleton<IVersionPublisher, TeamCityVersionPublisher>()
                                      .BuildServiceProvider();
    }

    private static void ApplyVersion(NuGetVersion version, IServiceProvider serviceProvider)
    {
        Console.WriteLine($"Version: {version}");

        IEnumerable<IVersionPublisher> publishers = serviceProvider.GetServices<IVersionPublisher>();

        foreach (IVersionPublisher publisher in publishers)
        {
            publisher.Publish(version);
        }
    }

    private static Repository OpenRepository(string workDir)
    {
        string found = Repository.Discover(workDir);

        return new(found);
    }

    private static int FindBuildNumber(int buildNumberFromCommandLine)
    {
        if (buildNumberFromCommandLine > 0)
        {
            Console.WriteLine($"Build number from command line: {buildNumberFromCommandLine}");

            return buildNumberFromCommandLine;
        }

        string? buildNumber = Environment.GetEnvironmentVariable(variable: "BUILD_NUMBER");

        if (!string.IsNullOrWhiteSpace(buildNumber))
        {
            Console.WriteLine($"Build number from TeamCity: {buildNumber}");

            if (int.TryParse(s: buildNumber, style: NumberStyles.Integer, provider: CultureInfo.InvariantCulture, out int build) && build >= 0)
            {
                return build;
            }
        }

        return 0;
    }
}