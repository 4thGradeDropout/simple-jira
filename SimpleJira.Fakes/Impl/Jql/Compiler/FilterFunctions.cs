using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Iveonik.Stemmers;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal static class FilterFunctions
    {
        private static readonly RussianStemmer stemmer = new RussianStemmer();

        private static readonly char[] separators = {',', '/', '\\', ';', ':', '-', '|'};

        private static readonly HashSet<string> russianPrepositions = new HashSet<string>(new[]
        {
            "в", "без", "до", "из", "к", "на", "по", "о", "от", "перед", "при", "через", "с", "у", "за", "над", "об",
            "под", "про", "для", "а", "но", "да"
        }, StringComparer.InvariantCultureIgnoreCase);

        public static bool Search(string value, string query)
        {
            var queryTokens = Tokenize(query, true);
            if (queryTokens.Length == 0)
                return false;
            var valueTokens = Tokenize(value, true);
            return queryTokens
                .All(x =>
                    valueTokens.Any(y => x.Contains('*')
                        ? WildCardToRegular(x).Match(y).Success
                        : string.Equals(stemmer.Stem(x), stemmer.Stem(y), StringComparison.InvariantCulture)));
        }

        public static bool SearchDirectly(string value, string query)
        {
            var normalizedQuery = string.Join(" ", Tokenize(query, false));
            var normalizedValue = string.Join(" ", Tokenize(value, false));
            return new Regex("\\b" + Regex.Escape(normalizedQuery) + "\\b", RegexOptions.IgnoreCase)
                .Match(normalizedValue)
                .Success;
        }

        public static bool Greater(object value1, object value2)
        {
            return Comparer.Default.Compare(value1, value2) > 0;
        }

        public static bool GreaterOrEquals(object value1, object value2)
        {
            return Comparer.Default.Compare(value1, value2) >= 0;
        }

        public static bool Less(object value1, object value2)
        {
            return Comparer.Default.Compare(value1, value2) < 0;
        }

        public static bool LessOrEquals(object value1, object value2)
        {
            return Comparer.Default.Compare(value1, value2) <= 0;
        }

        public static bool Equals(object value1, string value2)
        {
            return value1 != null && value1 switch
            {
                string actualStringValue => string.Equals(actualStringValue, value2,
                    StringComparison.InvariantCultureIgnoreCase),
                DateTime actualDateValue => actualDateValue == FilterParseHelpers.ParseJiraDate(value2),
                int actualIntValue => actualIntValue == int.Parse(value2, CultureInfo.InvariantCulture),
                decimal actualDecimalValue => actualDecimalValue ==
                                              decimal.Parse(value2, CultureInfo.InvariantCulture),
                float actualFloatValue => Math.Abs(actualFloatValue -
                                                   float.Parse(value2, CultureInfo.InvariantCulture)) < 0.0001,
                double actualDoubleValue => Math.Abs(actualDoubleValue -
                                                     double.Parse(value2, CultureInfo.InvariantCulture)) < 0.0001,
                long actualLongValue => actualLongValue == long.Parse(value2),
                JiraStatus actualStatusValue => actualStatusValue == value2,
                JiraIssueReference actualIssueReferenceValue => actualIssueReferenceValue == value2,
                JiraCustomFieldOption actualOptionValue => actualOptionValue == value2,
                JiraPriority actualPriorityValue => actualPriorityValue == value2,
                JiraProject actualProjectValue => actualProjectValue == value2,
                JiraUser actualUserValue => actualUserValue == value2,
                JiraIssueType actualIssueJiraType => actualIssueJiraType == value2,
                IEnumerable actualEnumerableValue => actualEnumerableValue.Cast<object>().Any(x => Equals(x, value2)),
                _ => false
            };
        }

        private static string[] Tokenize(string value, bool excludePrepositions)
        {
            return value.Trim()
                .Split(null)
                .SelectMany(x => x.Split(separators))
                .Select(x => x.Trim('.'))
                .Where(x => !string.IsNullOrEmpty(x))
                .Where(x => !excludePrepositions || !russianPrepositions.Contains(x))
                .ToArray();
        }

        private static Regex WildCardToRegular(string value)
        {
            return new Regex("^" + Regex.Escape(value).Replace("\\*", ".*") + "$",
                RegexOptions.IgnoreCase);
        }
    }
}