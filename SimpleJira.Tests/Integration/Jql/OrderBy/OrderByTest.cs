using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.OrderBy
{
    public class OrderByTest : FieldMatchingTestBase
    {
        [Test]
        public async Task Asc()
        {
            var jira = CreateJira();
            var issue15 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issue10 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 10
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[12346] asc",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(2));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue10.Key));
            Assert.That(response.Issues[1].Key, Is.EqualTo(issue15.Key));
        }

        [Test]
        public async Task Desc()
        {
            var jira = CreateJira();
            var issue15 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issue10 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 10
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[12346] desc",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(2));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue15.Key));
            Assert.That(response.Issues[1].Key, Is.EqualTo(issue10.Key));
        }

        [Test]
        public async Task Default()
        {
            var jira = CreateJira();
            var issue15 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issue10 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 10
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[12346]",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(2));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue10.Key));
            Assert.That(response.Issues[1].Key, Is.EqualTo(issue15.Key));
        }

        [Test]
        public async Task Several()
        {
            var jira = CreateJira();
            var issue15 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 10,
                DateTimeValue = new DateTime(2020, 11, 15)
            }, CancellationToken.None);
            var issue10 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 10,
                DateTimeValue = new DateTime(2020, 11, 10)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[12346], cf[12349]",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(2));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue10.Key));
            Assert.That(response.Issues[1].Key, Is.EqualTo(issue15.Key));
        }
    }
}