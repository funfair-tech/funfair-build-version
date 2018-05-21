using System;
using System.Diagnostics;

namespace BuildVersion
{
    internal static class ExecutableVersionInformation
    {
        public static string ProgramVersion()
        {
            return CommonVersion(typeof(Program));
        }

        private static string CommonVersion(Type type)
        {
            FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(type.Assembly.Location);
            return fileVersionInfo.ProductVersion;
        }
    }
}