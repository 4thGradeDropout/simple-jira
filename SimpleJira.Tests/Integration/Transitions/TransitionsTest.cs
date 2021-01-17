using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Fakes.Interface;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Transitions
{
    public class TransitionsTest : TestBase
    {
        private IJira jira;

        protected override void SetUp()
        {
            base.SetUp();
            jira = FakeJira.InMemory("http://fake.jira.int", User(),
                new JiraMetadataProvider(new[] {typeof(JiraCustomIssue)}));
        }

        [Test]
        public async Task DefaultStatus()
        {
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = Project(),
                IssueType = JiraCustomIssue.Metadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new [] {"status"}
            });
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Status, Is.Not.Null);
            Assert.That(response.Issues[0].Status.Id, Is.EqualTo(JiraCustomIssue.Metadata.Status.New.Id));
        }

        [Test]
        public async Task GetTransitions()
        {
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = Project(),
                IssueType = JiraCustomIssue.Metadata.IssueType,
                Status = JiraCustomIssue.Metadata.Status.New
            }, CancellationToken.None);
            var transitions = await jira.GetTransitionsAsync(reference, CancellationToken.None);
            Assert.That(transitions.Length, Is.EqualTo(1));
            Assert.That(transitions[0].Id, Is.EqualTo("1"));
            Assert.That(transitions[0].Name, Is.EqualTo("Done"));
        }

        [Test]
        public async Task Invoke()
        {
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = Project(),
                IssueType = JiraCustomIssue.Metadata.IssueType,
                Status = JiraCustomIssue.Metadata.Status.New
            }, CancellationToken.None);

            await jira.InvokeTransitionAsync(reference, "1", null, CancellationToken.None);

            var issues = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "KEY = " + reference.Key
            });
            Assert.That(issues.Issues.Length, Is.EqualTo(1));
            Assert.That(issues.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(issues.Issues[0].Status.Id, Is.EqualTo(JiraCustomIssue.Metadata.Status.Done.Id));
        }

        private static JiraProject Project()
        {
            return new JiraProject
            {
                Id = "1",
                Key = "TESTPROJECT",
                Name = "Тестовый проект",
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

        private static JiraUser User()
        {
            return new JiraUser
            {
                Active = true,
                Key = "user.key",
                Name = "user.name",
                Self = "https://jira.int/rest/api/2/user/user.key",
                AvatarUrls = new JiraAvatarUrls(),
                DisplayName = "Peter Smith",
                EmailAddress = "peter@jira.int",
                TimeZone = "some/zone"
            };
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            private class Workflow : IDefineWorkflow<JiraCustomIssue>
            {
                public void Build(IWorkflowBuilder<JiraCustomIssue> builder)
                {
                    builder.Type(Metadata.IssueType)
                        .DefaultStatus(Metadata.Status.New)
                        .Status(Metadata.Status.New, new[]
                        {
                            new JiraTransition
                            {
                                Id = "1",
                                Name = "Done",
                                To = Metadata.Status.Done
                            }
                        })
                        .Status(Metadata.Status.Done, new[]
                        {
                            new JiraTransition
                            {
                                Id = "2",
                                Name = "New",
                                To = Metadata.Status.New
                            }
                        });
                }
            }

            public static class Metadata
            {
                public static JiraIssueType IssueType => new JiraIssueType
                {
                    Id = "1",
                    Name = "Client",
                    Description = "Client",
                    SubTask = false
                };

                public static class Status
                {
                    public static JiraStatus New => new JiraStatus
                    {
                        Id = "1",
                        Name = "New",
                        Description = "New",
                        Self = "http://fake.jira.int/rest/2/status/1",
                    };

                    public static JiraStatus Done => new JiraStatus
                    {
                        Id = "2",
                        Name = "Done",
                        Description = "Done",
                        Self = "http://fake.jira.int/rest/2/status/2",
                    };
                }
            }
        }
    }
}