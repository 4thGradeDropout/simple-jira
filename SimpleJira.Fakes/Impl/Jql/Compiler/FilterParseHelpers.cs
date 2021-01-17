using System;
using System.Text.RegularExpressions;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal static class FilterParseHelpers
    {
        private static readonly Regex dateValueRegex =
            new Regex(@"^(\d{4})([-/])(\d{1,2})\2(\d{1,2})(\s(\d{2})\:(\d{2}))?$", RegexOptions.Compiled);

        public static DateTime? ParseJiraDate(string dateAsString)
        {
            var match = dateValueRegex.Match(dateAsString);
            if (match.Success)
            {
                if (!string.IsNullOrEmpty(match.Groups[6].Value) && !string.IsNullOrEmpty(match.Groups[7].Value))
                    return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value),
                        int.Parse(match.Groups[4].Value), int.Parse(match.Groups[6].Value),
                        int.Parse(match.Groups[7].Value), 0);
                return new DateTime(int.Parse(match.Groups[1].Value), int.Parse(match.Groups[3].Value),
                    int.Parse(match.Groups[4].Value));
            }

            return null;
        }
    }
}