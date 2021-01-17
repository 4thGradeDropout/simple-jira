using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.IssueFunction
{
    public class SubtasksOf : FieldMatchingTestBase
    {
        [Test]
        public async Task Success()
        {
            var jira = CreateJira();
            var parent = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "parent issue"
            }, CancellationToken.None);

            var child = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Parent = parent,
                Summary = "child parent"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"issueFunction in subtasksOf('KEY={parent.Key}')"
            }, CancellationToken.None);

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(child.Key));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("child parent"));
        }

        [Test]
        public async Task ParentIsNotMatched()
        {
            var jira = CreateJira();
            var parent = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "parent issue"
            }, CancellationToken.None);

            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Parent = parent,
                Summary = "child parent"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"issueFunction in subtasksOf('summary ~ somesummary')"
            }, CancellationToken.None);

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task HasNotParent()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "child parent"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "issueFunction in subtasksOf('summary ~ some')"
            }, CancellationToken.None);

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }
    }
}