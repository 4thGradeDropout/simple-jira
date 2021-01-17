using System;
using System.Collections.Generic;
using System.Linq;

namespace SimpleJira.Fakes.Impl.Jql.Compiler
{
    internal class JqlCommand
    {
        private readonly Func<JiraIssueDto[], JiraIssueDto, bool> filter;
        private readonly JqlOrdering[] ordering;

        public JqlCommand(Func<JiraIssueDto[], JiraIssueDto, bool> filter, JqlOrdering[] ordering)
        {
            this.filter = filter;
            this.ordering = ordering;
        }

        public JiraIssueDto[] Execute(JiraIssueDto[] issues)
        {
            return Query(issues).ToArray();
        }

        private IEnumerable<JiraIssueDto> Query(JiraIssueDto[] issues)
        {
            var result = filter != null
                ? issues.Where(issue => filter(issues, issue))
                : issues;
            if (ordering != null && ordering.Length > 0)
            {
                IOrderedEnumerable<JiraIssueDto> orderedEnumerable = null;
                foreach (var field in ordering)
                {
                    var currentField = field;
                    if (orderedEnumerable == null)
                    {
                        orderedEnumerable = field.Direction == JqlOrderingDirection.Asc
                            ? result.OrderBy(x => x.IssueFields.GetProperty(currentField.Field, currentField.Type))
                            : result.OrderByDescending(x =>
                                x.IssueFields.GetProperty(currentField.Field, currentField.Type));
                    }
                    else
                    {
                        orderedEnumerable = field.Direction == JqlOrderingDirection.Asc
                            ? orderedEnumerable.ThenBy(x =>
                                x.IssueFields.GetProperty(currentField.Field, currentField.Type))
                            : orderedEnumerable.ThenByDescending(x =>
                                x.IssueFields.GetProperty(currentField.Field, currentField.Type));
                    }
                }

                result = orderedEnumerable;
            }

            return result;
        }
    }
}