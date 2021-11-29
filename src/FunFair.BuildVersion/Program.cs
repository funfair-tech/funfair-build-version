using System;
using System.Collections.Generic;
using CommandLine;
using FunFair.BuildVersion.Detection;
using FunFair.BuildVersion.Detection.ExternalBranchLocators;
using FunFair.BuildVersion.Interfaces;
using FunFair.BuildVersion.Publishers;
using FunFair.BuildVersion.Services;
using LibGit2Sharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        private static int NotParsed(IEnumerable<Error> errors)
        {
            Console.WriteLine("Errors:");

            foreach (Error error in errors)
            {
                Console.WriteLine($" * {error.Tag}");
            }

            return ERROR;
        }

        private static int ParsedOk(Options options)
        {
            int buildNumber = FindBuildNumber(options.BuildNumber);

            string workDir = Environment.CurrentDirectory;

            using (Repository repo = OpenRepository(workDir))
            {
                IServiceProvider serviceProvider = Setup(options: options, repo: repo);

                IDiagnosticLogger logging = serviceProvider.GetRequiredService<IDiagnosticLogger>();
                IVersionDetector versionDetector = serviceProvider.GetRequiredService<IVersionDetector>();

                NuGetVersion version = versionDetector.FindVersion(buildNumber);

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

        public static int Main(params string[] args)
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                return Parser.Default.ParseArguments<Options>(args)
                             .MapResult(parsedFunc: ParsedOk, notParsedFunc: NotParsed);
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");
                Console.WriteLine($"ERROR: {exception.GetType().FullName}");
                Console.WriteLine($"ERROR: {exception.StackTrace}");

                Exception? inner = exception.InnerException;

                if (inner != null)
                {
                    Console.WriteLine($"ERROR: {inner.Message}");
                    Console.WriteLine($"ERROR: {inner.GetType().FullName}");
                    Console.WriteLine($"ERROR: {inner.StackTrace}");
                }

                return ERROR;
            }
        }

        private static IServiceProvider Setup(Options options, Repository repo)
        {
            IServiceCollection services = new ServiceCollection();
            DiagnosticLogger logger = new(options.WarningsAsErrors);

            IBranchSettings branchSettings = new BranchSettings(releaseSuffix: options.ReleaseSuffix, package: options.Package);

            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<IDiagnosticLogger>(logger);
            services.AddSingleton(typeof(ILogger<>), typeof(LoggerProxy<>));

            services.AddSingleton(branchSettings);
            services.AddSingleton(repo);
            services.AddSingleton<IBranchDiscovery, GitBranchDiscovery>();
            services.AddSingleton<IBranchClassification, BranchClassification>();
            services.AddSingleton<IVersionPublisher, GitHubActionsVersionPublisher>();
            services.AddSingleton<IVersionPublisher, TeamCityVersionPublisher>();
            services.AddSingleton<IVersionDetector, VersionDetector>();

            services.AddSingleton<IExternalBranchLocator, GitHubRefEnvironmentVariableBranchLocator>();
            services.AddSingleton<IExternalBranchLocator, GitBranchEnvironmentVariableBranchLocator>();

            IServiceProviderFactory<IServiceCollection> spf = new DefaultServiceProviderFactory();

            return spf.CreateServiceProvider(services);
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

            string? buildNumber = Environment.GetEnvironmentVariable(variable: @"BUILD_NUMBER");

            if (!string.IsNullOrWhiteSpace(buildNumber))
            {
                Console.WriteLine($"Build number from TeamCity: {buildNumber}");

                if (int.TryParse(s: buildNumber, out int build) && build >= 0)
                {
                    return build;
                }
            }

            return 0;
        }
    }
}