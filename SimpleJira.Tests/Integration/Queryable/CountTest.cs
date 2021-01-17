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
    public class CountTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task Test()
        {
            for (var i = 0; i < 1643; ++i)
                await jira.CreateIssueAsync(new JiraCustomIssue
                {
                    Project = TestMetadata.Project,
                    IssueType = TestMetadata.IssueType,
                    Summary = "Тестовая тема"
                }, CancellationToken.None);
            Assert.That(
                Source<JiraCustomIssue>().Count(x => JqlFunctions.Contains(x.Summary, "тема")),
                Is.EqualTo(1643));
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