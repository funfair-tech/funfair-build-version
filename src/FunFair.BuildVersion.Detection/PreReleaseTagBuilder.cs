using System;
using System.Globalization;
using System.Linq;
using System.Text;
using FunFair.BuildVersion.Interfaces;

namespace FunFair.BuildVersion.Detection;

internal static class PreReleaseTagBuilder
{
    private const string INVALID_CHAR_STRING_REPLACEMENT = "-";
    private const char INVALID_CHAR_REPLACEMENT = '-';

    private const string DOUBLE_INVALID_REPLACEMENT_CHAR = INVALID_CHAR_STRING_REPLACEMENT + INVALID_CHAR_STRING_REPLACEMENT;

    public static StringBuilder NormalizeSourceBranchName(this string currentBranch, IBranchClassification branchClassification)
    {
        return new(branchClassification.IsPullRequest(currentBranch: currentBranch, out long pullRequestId)
                       ? "pr-" + pullRequestId.ToString(CultureInfo.InvariantCulture)
                       : currentBranch.ToLowerInvariant());
    }

    public static StringBuilder ReplaceInvalidCharacters(this StringBuilder suffix)
    {
        foreach (char ch in suffix.ToString()
                                  .Where(predicate: c => !char.IsLetterOrDigit(c) && c != INVALID_CHAR_REPLACEMENT)
                                  .Distinct())
        {
            suffix = suffix.Replace(oldChar: ch, newChar: INVALID_CHAR_REPLACEMENT);
        }

        return suffix;
    }

    public static StringBuilder RemoveFirstFolderInBranchName(this StringBuilder suffix)
    {
        int pos = suffix.ToString()
                        .IndexOf(value: '/', comparisonType: StringComparison.Ordinal);

        if (pos != -1)
        {
            suffix = suffix.Remove(startIndex: 0, pos + 1);
        }

        return suffix;
    }

    public static StringBuilder RemoveDoubleHyphens(this StringBuilder suffix)
    {
        while (suffix.ToString()
                     .Contains(value: DOUBLE_INVALID_REPLACEMENT_CHAR, comparisonType: StringComparison.Ordinal))
        {
            suffix = suffix.Replace(oldValue: DOUBLE_INVALID_REPLACEMENT_CHAR, newValue: INVALID_CHAR_STRING_REPLACEMENT);
        }

        return suffix;
    }

    public static StringBuilder RemoveLeadingDigits(this StringBuilder suffix)
    {
        while (suffix.Length != 0)
        {
            if (!char.IsDigit(suffix[0]))
            {
                break;
            }

            suffix = suffix.Remove(startIndex: 0, length: 1);
        }

        return suffix;
    }

    public static string EnsureNotBlank(this StringBuilder suffix)
    {
        string usedSuffix = suffix.ToString()
                                  .TrimStart(INVALID_CHAR_REPLACEMENT);

        if (string.IsNullOrWhiteSpace(usedSuffix))
        {
            return "prerelease";
        }

        return usedSuffix;
    }

    public static string EnsureNotTooLong(this string usedSuffix)
    {
        const int maxSuffixLength = 15;

        if (usedSuffix.Length > maxSuffixLength)
        {
            usedSuffix = usedSuffix.Substring(startIndex: 0, length: maxSuffixLength);
        }

        // Ensure that the name doesn't end with a -
        return usedSuffix.Trim(trimChar: '-');
    }
}