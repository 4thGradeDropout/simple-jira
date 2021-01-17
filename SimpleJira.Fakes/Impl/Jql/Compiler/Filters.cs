using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal static class Filters
    {
        private static readonly Dictionary<Type, Func<string, object>> comparableTypeParsers =
            new Dictionary<Type, Func<string, object>>
            {
                {typeof(int), s => int.Parse(s, CultureInfo.InvariantCulture)},
                {typeof(long), s => long.Parse(s, CultureInfo.InvariantCulture)},
                {typeof(decimal), s => decimal.Parse(s, CultureInfo.InvariantCulture)},
                {typeof(float), s => float.Parse(s, CultureInfo.InvariantCulture)},
                {typeof(double), s => double.Parse(s, CultureInfo.InvariantCulture)},
                {typeof(DateTime), s => FilterParseHelpers.ParseJiraDate(s)},
            };

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Equals(string propertyName, Type propertyType,
            string value)
        {
            if (value == null)
                throw new InvalidOperationException($"comparable value for property [{propertyName}] can not be null");

            var type = propertyType;
            if (type.IsArray)
                type = type.GetElementType() ?? typeof(object);
            type = Nullable.GetUnderlyingType(type) ?? type;
            if (comparableTypeParsers.TryGetValue(type, out var parser))
                ParseOrDie(propertyName, type, value, parser);

            return (issues, issue) => Equals(issue, propertyName, propertyType, value);
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> In(string propertyName, Type propertyType,
            string[] values)
        {
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (comparableTypeParsers.TryGetValue(type, out var parser))
                foreach (var value in values)
                    ParseOrDie(propertyName, type, value, parser);

            return (issues, issue) =>
            {
                var result = false;
                for (var i = 0; !result && i < values.Length; i++)
                    result |= Equals(issue, propertyName, propertyType, values[i]);
                return result;
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Greater(string propertyName, Type propertyType,
            string value)
        {
            return Compare(propertyName, propertyType, value, FilterFunctions.Greater);
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> GreaterOrEquals(string propertyName, Type propertyType,
            string value)
        {
            return Compare(propertyName, propertyType, value, FilterFunctions.GreaterOrEquals);
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Less(string propertyName, Type propertyType,
            string value)
        {
            return Compare(propertyName, propertyType, value, FilterFunctions.Less);
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> LessOrEquals(string propertyName, Type propertyType,
            string value)
        {
            return Compare(propertyName, propertyType, value, FilterFunctions.LessOrEquals);
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Contains(string propertyName, Type propertyType,
            string value)
        {
            var directly = false;
            if (string.IsNullOrEmpty(value))
                throw new JqlCompilationException($"value of the field [{propertyName}] can not be null or empty");
            if (propertyType != typeof(string))
                throw new JqlCompilationException(
                    $"can not use ~ operator for field [{propertyName}] because field type is not [String]");
            if (value[0] == '\'')
                value = value.Trim('\'').Replace("\'\'", "'");
            else if (value[0] == '\"' || value[value.Length - 1] == '\"')
                try
                {
                    value = StringHelpers.Unescape(value);
                    directly = true;
                }
                catch (Exception)
                {
                    throw new JqlCompilationException(
                        $"value [{value}] has incorrect format, field [{propertyName}]");
                }

            return (issues, issue) =>
            {
                var actualValue = issue.IssueFields.GetProperty<string>(propertyName);
                if (actualValue == null)
                    return false;
                return directly
                    ? FilterFunctions.SearchDirectly(actualValue, value)
                    : FilterFunctions.Search(actualValue, value);
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> IsEmpty(string propertyName, Type propertyType)
        {
            return (issues, issue) =>
            {
                var actualValue = issue.IssueFields.GetProperty(propertyName, propertyType);
                return actualValue is string stringValue ? string.IsNullOrEmpty(stringValue) : actualValue == null;
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> ParentsOf(
            Func<JiraIssueDto[], JiraIssueDto, bool> filter)
        {
            return (issues, parent) =>
            {
                var result = false;
                for (var i = 0; !result && i < issues.Length; i++)
                {
                    var child = issues[i];
                    if (Equals(child.Key, parent.Key) || Equals(child.Key, parent.Key))
                        continue;
                    var parentReference = child.IssueFields.GetProperty<JiraIssueReference>("parent");
                    if (parentReference == (JiraIssueReference) null)
                        continue;
                    if (Equals(parent.Key, parentReference.Key) || Equals(parent.Id, parentReference.Id))
                        result = filter(issues, child);
                }

                return result;
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> SubTasksOf(
            Func<JiraIssueDto[], JiraIssueDto, bool> filter)
        {
            return (issues, child) =>
            {
                var parentReference = child.IssueFields.GetProperty<JiraIssueReference>("parent");
                if (parentReference == (JiraIssueReference) null)
                    return false;
                var result = false;
                for (var i = 0; !result && i < issues.Length; ++i)
                {
                    var parent = issues[i];
                    if (Equals(parent.Key, parentReference.Key) || Equals(parent.Id, parentReference.Id))
                        result = filter(issues, parent);
                }

                return result;
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> InCascadeOption(string propertyName, Type propertyType,
            string[] values)
        {
            if (propertyType != typeof(JiraCustomFieldOption))
                throw new JqlCompilationException(
                    $"cascade option filter is not supported for field [{propertyName}] because its type is not [option]");
            return (issues, issue) =>
            {
                var currentOption = issue.IssueFields.GetProperty<JiraCustomFieldOption>(propertyName);
                var i = 0;
                while (i < values.Length && currentOption != (JiraCustomFieldOption) null)
                {
                    if (currentOption != values[i])
                        return false;
                    currentOption = currentOption.Child;
                    ++i;
                }

                return i == values.Length;
            };
        }

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Not(Func<JiraIssueDto[], JiraIssueDto, bool> filter) =>
            (issues, issue) => !filter(issues, issue);

        public static Func<JiraIssueDto[], JiraIssueDto, bool> And(
            Func<JiraIssueDto[], JiraIssueDto, bool> left, Func<JiraIssueDto[], JiraIssueDto, bool> right) =>
            (issues, issue) => left(issues, issue) && right(issues, issue);

        public static Func<JiraIssueDto[], JiraIssueDto, bool> Or(
            Func<JiraIssueDto[], JiraIssueDto, bool> left, Func<JiraIssueDto[], JiraIssueDto, bool> right) =>
            (issues, issue) => left(issues, issue) || right(issues, issue);

        private static bool Equals(JiraIssueDto issue, string propertyName, Type propertyType, string value)
        {
            if (Equals(propertyName, "key"))
                return Equals(issue.Key, value);
            if (Equals(propertyName, "id"))
                return Equals(issue.Id, value);
            if (Equals(propertyName, "self"))
                return Equals(issue.Self, value);
            if (Equals(propertyName, "statuscategory"))
            {
                var actualStatus = issue.IssueFields.GetProperty<JiraStatus>("status");
                return actualStatus != (JiraStatus) null && actualStatus.StatusCategory == value;
            }
            var actualValue = issue.IssueFields.GetProperty(propertyName, propertyType);
            return FilterFunctions.Equals(actualValue, value);
        }

        private static Func<JiraIssueDto[], JiraIssueDto, bool> Compare(string propertyName, Type propertyType,
            string value, Func<object, object, bool> comparer)
        {
            if (value == null)
                throw new JqlCompilationException($"comparable value for field [{propertyName}] can not be null");
            var type = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
            if (!comparableTypeParsers.TryGetValue(type, out var parser))
                throw new JqlCompilationException(
                    $"field [{propertyName}] has incomparable type [{type.Name}]");
            var parsedValue = ParseOrDie(propertyName, propertyType, value, parser);
            return (issues, issue) =>
            {
                var actualValue = issue.IssueFields.GetProperty(propertyName, propertyType);
                return actualValue != null && comparer(actualValue, parsedValue);
            };
        }

        private static object ParseOrDie(string propertyName, Type propertyType, string value,
            Func<string, object> parser)
        {
            try
            {
                return parser(value);
            }
            catch (Exception)
            {
                throw new JqlCompilationException(
                    $"comparable value [{value}] can not be converted to type [{propertyType}], field [{propertyName}]");
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Equals(string s1, string s2) =>
            string.Equals(s1, s2, StringComparison.InvariantCultureIgnoreCase);
    }
}