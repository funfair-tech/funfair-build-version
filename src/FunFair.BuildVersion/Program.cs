using System;
using System.Collections.Generic;
using System.Globalization;
using CommandLine;
using FunFair.BuildVersion.Detection;
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

    private static int NotParsed(IEnumerable<Error> errors)
    {
        Console.WriteLine("Errors:");

        foreach (Error error in errors)
        {
            Console.WriteLine($" * {error.Tag.GetName()}");
        }

        return ERROR;
    }

    private static int ParsedOk(Options options)
    {
        int buildNumber = FindBuildNumber(options.BuildNumber);

        string workDir = Environment.CurrentDirectory;

        using (Repository repository = OpenRepository(workDir))
        {
            IServiceProvider serviceProvider = Setup(options: options);

            IDiagnosticLogger logging = serviceProvider.GetRequiredService<IDiagnosticLogger>();
            IVersionDetector versionDetector =
                serviceProvider.GetRequiredService<IVersionDetector>();

            NuGetVersion version = versionDetector.FindVersion(
                repository: repository,
                buildNumber: buildNumber
            );

            ApplyVersion(version: version, serviceProvider: serviceProvider);

            if (logging.IsErrored)
            {
                Console.WriteLine();
                Console.WriteLine(
                    logging.Errors > 1
                        ? $"Found {logging.Errors} Errors"
                        : $"Found {logging.Errors} Error"
                );

                return ERROR;
            }

            return SUCCESS;
        }
    }

    public static int Main(params string[] args)
    {
        try
        {
            Console.WriteLine($"{VersionInformation.Product} {VersionInformation.Version}");

            return Parser
                .Default.ParseArguments<Options>(args)
                .MapResult(parsedFunc: ParsedOk, notParsedFunc: NotParsed);
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

        IBranchSettings branchSettings = new BranchSettings(
            releaseSuffix: options.ReleaseSuffix,
            package: options.Package
        );

        return new ServiceCollection()
            .AddSingleton<ILogger>(logger)
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

        IEnumerable<IVersionPublisher> publishers =
            serviceProvider.GetServices<IVersionPublisher>();

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

            if (
                int.TryParse(
                    s: buildNumber,
                    style: NumberStyles.Integer,
                    provider: CultureInfo.InvariantCulture,
                    out int build
                )
                && build >= 0
            )
            {
                return build;
            }
        }

        return 0;
    }
}
