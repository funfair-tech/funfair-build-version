using System;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.Tests;

[Collection("ProgramMainTests")]
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
        int result = RunWithBuildNumberEnvVar(value: "55", act: () => FunFair.BuildVersion.Program.FindBuildNumber(0));

        Assert.Equal(expected: 55, actual: result);
    }

    [Fact]
    public void NegativeCommandLineFallsBackToEnvironmentVariableWhenSet()
    {
        int result = RunWithBuildNumberEnvVar(value: "77", act: () => FunFair.BuildVersion.Program.FindBuildNumber(-1));

        Assert.Equal(expected: 77, actual: result);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-5)]
    [InlineData(int.MinValue)]
    public void NonPositiveCommandLineReturnsZeroWhenEnvironmentVariableNotSet(int buildNumber)
    {
        int result = RunWithBuildNumberEnvVar(
            value: null,
            act: () => FunFair.BuildVersion.Program.FindBuildNumber(buildNumber)
        );

        Assert.Equal(expected: 0, actual: result);
    }

    [Theory]
    [InlineData("not-a-number")]
    [InlineData("   ")]
    public void UnparseableEnvironmentVariableReturnsZero(string envVarValue)
    {
        int result = RunWithBuildNumberEnvVar(
            value: envVarValue,
            act: () => FunFair.BuildVersion.Program.FindBuildNumber(0)
        );

        Assert.Equal(expected: 0, actual: result);
    }

    [Fact]
    public void EnvironmentVariableSetToZeroReturnsZero()
    {
        int result = RunWithBuildNumberEnvVar(value: "0", act: () => FunFair.BuildVersion.Program.FindBuildNumber(0));

        Assert.Equal(expected: 0, actual: result);
    }

    private static int RunWithBuildNumberEnvVar(string? value, Func<int> act)
    {
        string? previousValue = Environment.GetEnvironmentVariable(BuildNumberEnvVar);

        try
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: value);

            return act();
        }
        finally
        {
            Environment.SetEnvironmentVariable(variable: BuildNumberEnvVar, value: previousValue);
        }
    }
}
