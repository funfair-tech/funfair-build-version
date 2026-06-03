using System;
using FunFair.BuildVersion.GitTagBuildNumber.Extensions;
using FunFair.Test.Common;
using Xunit;

namespace FunFair.BuildVersion.GitTagBuildNumber.Tests.Extensions;

public sealed class UriExtensionsTests : TestBase
{
    [Theory]
    [InlineData("http://example.com", true)]
    [InlineData("https://example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("ssh://example.com", false)]
    [InlineData("file:///some/path", false)]
    public void IsHttpShouldReturnExpectedResult(string uriString, bool expected)
    {
        Uri uri = new(uriString);

        bool result = uri.IsHttp();

        Assert.Equal(expected: expected, actual: result);
    }
}
