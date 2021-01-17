using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.IsEmpty
{
    public class Negative : FieldMatchingTestBase
    {
        [Test]
        public async Task Success()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                StringValue = "some value"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12345] is not empty",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].StringValue, Is.EqualTo("some value"));
        }

        [Test]
        public async Task NotSuccess()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12345] is not empty",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }
    }
}