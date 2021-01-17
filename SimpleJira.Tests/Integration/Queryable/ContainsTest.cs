using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class ContainsTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task True()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Тестовая тема"
            }, CancellationToken.None);
            AssertSingle(Source<JiraCustomIssue>()
                    .Where(x => JqlFunctions.Contains(x.Summary, "тема")),
                reference);
        }

        [Test]
        public async Task False()
        {
            await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Тестовая тема"
            }, CancellationToken.None);
            AssertEmpty(Source<JiraCustomIssue>()
                .Where(x => JqlFunctions.Contains(x.Summary, Guid.NewGuid().ToString())));
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }
        }
    }
}