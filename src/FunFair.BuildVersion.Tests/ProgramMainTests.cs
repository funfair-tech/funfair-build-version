using System;
using System.IO;
using System.Threading.Tasks;
using FunFair.BuildVersion.Tests.Helpers;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.Tests;

[Collection("ProgramMainTests")]
public sealed class ProgramMainTests : TestBase
{
    private static readonly string[] UnrecognisedArgs = ["--definitely-not-a-valid-flag"];
    private static readonly string[] ValidBuildNumberArgs = ["--BuildNumber", "1"];

    [Fact]
    public async Task MainWithUnrecognisedArgs_ReturnsError()
    {
        using ConsoleCapture capture = new();

        int result = await FunFair.BuildVersion.Program.Main(UnrecognisedArgs);

        Assert.Equal(expected: 1, actual: result);
        Assert.Contains(
            expectedSubstring: "Errors:",
            actualString: capture.StdOut,
            comparisonType: StringComparison.Ordinal
        );
    }

    [Fact]
    public async Task MainWithValidBuildNumber_InGitRepo_ReturnsSuccess()
    {
        using ConsoleCapture capture = new();

        int result = await FunFair.BuildVersion.Program.Main(ValidBuildNumberArgs);

        Assert.Equal(expected: 0, actual: result);
    }

    [Fact]
    public async Task MainWithExceptionFromOpenRepository_CatchesAndReturnsError()
    {
        string originalDirectory = Environment.CurrentDirectory;

        try
        {
            Environment.CurrentDirectory = Path.GetTempPath();

            using ConsoleCapture capture = new();

            int result = await FunFair.BuildVersion.Program.Main(ValidBuildNumberArgs);

            Assert.Equal(expected: 1, actual: result);
        }
        finally
        {
            Environment.CurrentDirectory = originalDirectory;
        }
    }
}
