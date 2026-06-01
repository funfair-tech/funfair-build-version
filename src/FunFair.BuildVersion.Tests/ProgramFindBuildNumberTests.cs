using System;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.Tests;

public sealed class ProgramFindBuildNumberTests : TestBase
{
    private const string BuildNumberEnvVar = "BUILD_NUMBER";

    [Theory]
    [InlineData(1)]
    [InlineData(42)]
    [InlineData(int.MaxValue)]
    public void PositiveCommandLineValueIsReturnedDirectly(int buildNumber)
    {
        int result = FunFair.BuildVersion.Program.FindBuildNumber(buildNumber);

        Assert.Equal(expected: buildNumber, actual: result);
    }

    [Fact]
    public void ZeroCommandLineFallsBackToEnvironmentVariableWhenSet()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: "55");

            int result = FunFair.BuildVersion.Program.FindBuildNumber(0);

            Assert.Equal(expected: 55, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void NegativeCommandLineFallsBackToEnvironmentVariableWhenSet()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: "77");

            int result = FunFair.BuildVersion.Program.FindBuildNumber(-1);

            Assert.Equal(expected: 77, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void ZeroCommandLineReturnsZeroWhenEnvironmentVariableNotSet()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: null);

            int result = FunFair.BuildVersion.Program.FindBuildNumber(0);

            Assert.Equal(expected: 0, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void NegativeCommandLineReturnsZeroWhenEnvironmentVariableNotSet()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: null);

            int result = FunFair.BuildVersion.Program.FindBuildNumber(-5);

            Assert.Equal(expected: 0, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void InvalidEnvironmentVariableReturnsZero()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: "not-a-number");

            int result = FunFair.BuildVersion.Program.FindBuildNumber(0);

            Assert.Equal(expected: 0, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void WhitespaceOnlyEnvironmentVariableReturnsZero()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: "   ");

            int result = FunFair.BuildVersion.Program.FindBuildNumber(0);

            Assert.Equal(expected: 0, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }

    [Fact]
    public void EnvironmentVariableSetToZeroReturnsZero()
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: "0");

            int result = FunFair.BuildVersion.Program.FindBuildNumber(0);

            Assert.Equal(expected: 0, actual: result);
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }
}
