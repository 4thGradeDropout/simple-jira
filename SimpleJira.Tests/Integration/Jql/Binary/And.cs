using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.Binary
{
    public class And : FieldMatchingTestBase
    {
        [Test]
        public async Task Success()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "some subject"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"summary ~ \"some\" and key = {issue.Key}",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("some subject"));
        }

        [Test]
        public async Task NotSuccess_FirstCondition()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "some subject"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"summary ~ \"else\" and key = {issue.Key}",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task NotSuccess_SecondCondition()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "some subject"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"summary ~ \"some\" and key = {Guid.NewGuid()}",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }
    }
}