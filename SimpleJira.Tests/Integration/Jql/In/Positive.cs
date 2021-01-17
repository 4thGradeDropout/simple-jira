using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.In
{
    public class Positive : FieldMatchingTestBase
    {
        [Test]
        public async Task Single_Success()
        {
            var jira = CreateJira();
            var key = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 34
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12346] in (34)",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(34));
        }

        [Test]
        public async Task Single_NotSuccess()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 34
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12346] in (35)",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Several_Success()
        {
            var jira = CreateJira();
            var key = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 34
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12346] in (34, 35)",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(34));
        }

        [Test]
        public async Task Several_NotSuccess()
        {
            var jira = CreateJira();
            var key = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 34
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12346] in (35, 36)",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }
    }
}