using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Jql.FieldMatching
{
    public class NotEquals : FieldMatchingTestBase
    {
        [Test]
        public async Task Test()
        {
            var jira = CreateJira();
            var issue1 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = Project1(),
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            var issue2 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = Project2(),
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY != {issue1.Key}",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue2.Key));
        }

        private static JiraProject Project1()
        {
            return new JiraProject
            {
                Id = "1",
                Key = "TESTPROJECT1",
                Name = "Тестовый проект 1",
                ProjectTypeKey = "typekey",
                Self = "https://jira.int/rest/api/2/project/1",
                AvatarUrls = new JiraAvatarUrls
                {
                    Size16x16 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13304",
                    Size24x24 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13305",
                    Size32x32 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13306",
                    Size48x48 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13307",
                }
            };
        }

        private static JiraProject Project2()
        {
            return new JiraProject
            {
                Id = "2",
                Key = "TESTPROJECT2",
                Name = "Тестовый проект 2",
                ProjectTypeKey = "typekey",
                Self = "https://jira.int/rest/api/2/project/2",
                AvatarUrls = new JiraAvatarUrls
                {
                    Size16x16 = "https://jira.int/secure/projectavatar?size=xsmall&pid=2&avatarId=13308",
                    Size24x24 = "https://jira.int/secure/projectavatar?size=xsmall&pid=2&avatarId=13309",
                    Size32x32 = "https://jira.int/secure/projectavatar?size=xsmall&pid=2&avatarId=13310",
                    Size48x48 = "https://jira.int/secure/projectavatar?size=xsmall&pid=2&avatarId=13311",
                }
            };
        }
    }
}