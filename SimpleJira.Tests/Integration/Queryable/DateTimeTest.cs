using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class DateTimeTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new Type[0];
        }

        [Test]
        public async Task Test()
        {
            var now = DateTime.Now;
            await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DueDate = now
            }, CancellationToken.None);

            var issues = Source<JiraIssue>().ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].DueDate, Is.EqualTo(now));
        }
    }
}