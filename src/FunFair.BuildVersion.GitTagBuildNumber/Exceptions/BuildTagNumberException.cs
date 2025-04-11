using System;

namespace FunFair.BuildVersion.GitTagBuildNumber.Exceptions;

public sealed class BuildTagNumberException : Exception
{
    public BuildTagNumberException() { }

    public BuildTagNumberException(string? message)
        : base(message) { }

    public BuildTagNumberException(string? message, Exception? innerException)
        : base(message: message, innerException: innerException) { }
}
