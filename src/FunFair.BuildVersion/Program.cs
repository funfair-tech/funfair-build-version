using System;
using System.Collections.Generic;
using FunFair.BuildVersion.Detection;
using FunFair.BuildVersion.Interfaces;
using FunFair.BuildVersion.Publishers;
using FunFair.BuildVersion.Services;
using LibGit2Sharp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Versioning;

namespace FunFair.BuildVersion
{
    internal static class Program
    {
        private const int SUCCESS = 0;
        private const int ERROR = 1;

        public static int Main(params string[] args)
        {
            try
            {
                Console.WriteLine($"{typeof(Program).Namespace} {ExecutableVersionInformation.ProgramVersion()}");

                IConfigurationRoot configuration = new ConfigurationBuilder()
                                                   .AddCommandLine(args: args, new Dictionary<string, string> {{@"-WarningAsErrors", @"WarningAsErrors"}, {@"-BuildNumber", @"BuildNumber"}})
                                                   .Build();
                bool warningsAsErrors = configuration.GetValue<bool>(key: @"WarningAsErrors");

                int buildNumber = FindBuildNumber(configuration.GetValue(key: @"BuildNumber", defaultValue: -1));

                if (warningsAsErrors)
                {
                    Console.WriteLine(value: "** Running with Warnings as Errors");
                }

                string workDir = Environment.CurrentDirectory;

                using (Repository repo = OpenRepository(workDir))
                {
                    IServiceProvider serviceProvider = Setup(warningsAsErrors: warningsAsErrors, repo: repo);

                    IDiagnosticLogger logging = serviceProvider.GetRequiredService<IDiagnosticLogger>();
                    IVersionDetector versionDetector = serviceProvider.GetRequiredService<IVersionDetector>();

                    NuGetVersion? version = versionDetector.FindVersion(buildNumber);

                    if (version == null)
                    {
                        return ERROR;
                    }

                    ApplyVersion(version: version, serviceProvider: serviceProvider);

                    if (logging.IsErrored)
                    {
                        Console.WriteLine();
                        Console.WriteLine(logging.Errors > 1 ? $"Found {logging.Errors} Errors" : $"Found {logging.Errors} Error");

                        return ERROR;
                    }

                    return SUCCESS;
                }
            }
            catch (Exception exception)
            {
                Console.WriteLine($"ERROR: {exception.Message}");

                return ERROR;
            }
        }

        private static IServiceProvider Setup(bool warningsAsErrors, Repository repo)
        {
            IServiceCollection services = new ServiceCollection();
            DiagnosticLogger logger = new(warningsAsErrors);
            services.AddSingleton<ILogger>(logger);
            services.AddSingleton<IDiagnosticLogger>(logger);
            services.AddSingleton(typeof(ILogger<>), typeof(LoggerProxy<>));

            services.AddSingleton(repo);
            services.AddSingleton<IBranchDiscovery, GitBranchDiscovery>();
            services.AddSingleton<IBranchClassification, BranchClassification>();
            services.AddSingleton<IPullRequest, PullRequest>();
            services.AddSingleton<IVersionPublisher, GitHubActionsVersionPublisher>();
            services.AddSingleton<IVersionPublisher, TeamCityVersionPublisher>();
            services.AddSingleton<IVersionDetector, VersionDetector>();

            IServiceProviderFactory<IServiceCollection> spf = new DefaultServiceProviderFactory();

            return spf.CreateServiceProvider(services);
        }

        private static void ApplyVersion(NuGetVersion version, IServiceProvider serviceProvider)
        {
            Console.WriteLine($"Version: {version}");

            IEnumerable<IVersionPublisher> publishers = serviceProvider.GetServices<IVersionPublisher>();

            foreach (var publisher in publishers)
            {
                publisher.Publish(version);
            }
        }

        private static Repository OpenRepository(string workDir)
        {
            string found = Repository.Discover(workDir);

            return new Repository(found);
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