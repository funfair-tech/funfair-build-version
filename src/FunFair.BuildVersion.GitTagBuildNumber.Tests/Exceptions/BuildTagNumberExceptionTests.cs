using System;
using FunFair.BuildVersion.GitTagBuildNumber.Exceptions;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Exceptions;

public sealed class BuildTagNumberExceptionTests : TestBase
{
    [Fact]
    public void DefaultConstructorShouldCreateException()
    {
        BuildTagNumberException exception = new();

        Assert.NotNull(exception);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void MessageConstructorShouldSetMessage()
    {
        const string message = "Test error message";

        BuildTagNumberException exception = new(message);

        Assert.Equal(expected: message, actual: exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void MessageAndInnerExceptionConstructorShouldSetBoth()
    {
        const string message = "Outer error";
        InvalidOperationException innerException = new("Inner error");

        BuildTagNumberException exception = new(message: message, innerException: innerException);

        Assert.Equal(expected: message, actual: exception.Message);
        Assert.Same(expected: innerException, actual: exception.InnerException);
    }

    [Fact]
    public void MessageAndNullInnerExceptionConstructorShouldWork()
    {
        const string message = "Error with no inner";

        BuildTagNumberException exception = new(message: message, innerException: null);

        Assert.Equal(expected: message, actual: exception.Message);
        Assert.Null(exception.InnerException);
    }
}
