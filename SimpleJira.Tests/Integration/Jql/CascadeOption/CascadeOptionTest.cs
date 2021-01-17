using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;
using SimpleJira.Tests.Integration.Jql.FieldMatching;

namespace SimpleJira.Tests.Integration.Jql.CascadeOption
{
    public class CascadeOptionTest : FieldMatchingTestBase
    {
        [Test]
        public async Task First_Including()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                CustomField = Option()
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12350] in cascadeOption(parent)",
                StartAt = 0,
                MaxResults = 5000
            }, CancellationToken.None);

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
        }

        [Test]
        public async Task Second_Including()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                CustomField = Option()
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "cf[12350] in cascadeOption(parent, child)",
                StartAt = 0,
                MaxResults = 5000
            }, CancellationToken.None);

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
        }

        private static JiraCustomFieldOption Option()
        {
            return new JiraCustomFieldOption
            {
                Id = "12364",
                Value = "parent",
                Self = "https://jira.int/rest/api/2/customfield/12364",
                Child = new JiraCustomFieldOption
                {
                    Id = "12365",
                    Value = "child",
                    Self = "https://jira.int/rest/api/2/customfield/12365",
                }
            };
        }
    }
}