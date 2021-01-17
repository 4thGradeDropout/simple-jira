using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class EmptyFilterTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new Type[0];
        }

        [Test]
        public async Task Test()
        {
            var issue = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraIssue>().ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Key, Is.EqualTo(issue.Key));
        }
    }
}