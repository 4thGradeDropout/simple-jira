using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Fakes.Interface;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration
{
    public class MockJira : TestBase
    {
        private IMockJira fileJira;
        private IMockJira memoryJira;

        protected override void SetUp()
        {
            base.SetUp();
            var metadataProvider = new JiraMetadataProvider(new[] {typeof(JiraCustomIssue)});
            var folderPath = Path.Combine(Path.GetTempPath(), "fileJiraImplementation");
            fileJira = FakeJira.File(folderPath, "http://fake.jira.int", TestMetadata.User,
                metadataProvider);
            memoryJira = FakeJira.InMemory("http://fake.jira.int", TestMetadata.User,
                metadataProvider);
            fileJira.Drop();
            memoryJira.Drop();
        }

        protected override void TearDown()
        {
            try
            {
                fileJira.Drop();
                memoryJira.Drop();
            }
            finally
            {
                base.TearDown();
            }
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task CreateIssue(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test issue"
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync(new JiraIssuesRequest
            {
                Jql = "summary ~ Test",
                Fields = new[] {"summary"}
            });
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Id, Is.EqualTo(reference.Id));
            Assert.That(response.Issues[0].Self, Is.EqualTo(reference.Self));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("Test issue"));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task UpdateIssue(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test issue"
            }, CancellationToken.None);

            await jira.UpdateIssueAsync(reference, new JiraIssue
            {
                Assignee = new JiraUser
                {
                    Key = "testUser"
                }
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync(new JiraIssuesRequest
            {
                Jql = "summary ~ Test",
                Fields = new[] {"summary", "assignee"}
            });
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Id, Is.EqualTo(reference.Id));
            Assert.That(response.Issues[0].Self, Is.EqualTo(reference.Self));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("Test issue"));
            Assert.That(response.Issues[0].Assignee, Is.Not.Null);
            Assert.That(response.Issues[0].Assignee.Key, Is.EqualTo("testUser"));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task AddComment(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test issue"
            }, CancellationToken.None);

            await jira.AddCommentAsync(reference, new JiraComment
            {
                Body = "Some comment"
            }, CancellationToken.None);

            var response = await jira.GetCommentsAsync(reference, new JiraCommentsRequest
            {
                StartAt = 0,
                MaxResults = 200
            }, CancellationToken.None);
            Assert.That(response.Total, Is.EqualTo(1));
            Assert.That(response.StartAt, Is.EqualTo(0));
            Assert.That(response.Comments.Length, Is.EqualTo(1));
            Assert.That(response.Comments[0].Body, Is.EqualTo("Some comment"));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task UploadAttachment(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test issue"
            }, CancellationToken.None);

            var attachment = await jira.UploadAttachmentAsync(reference, "some.txt",
                Encoding.UTF8.GetBytes("some text"),
                CancellationToken.None);

            var downloadedAttachment = await jira.DownloadAttachmentAsync(attachment, CancellationToken.None);

            Assert.That(downloadedAttachment, Is.Not.Null);
            Assert.That(Encoding.UTF8.GetString(downloadedAttachment), Is.EqualTo("some text"));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task CreateIssueWithScopeInitialization(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue",
                IntValue = 256
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new[] {"project", "issueType", "summary", "customfield_43567"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Project, Is.Not.Null);
            Assert.That(response.Issues[0].Project.Id, Is.EqualTo(TestMetadata.Project.Id));
            Assert.That(response.Issues[0].IssueType, Is.Not.Null);
            Assert.That(response.Issues[0].IssueType.Id, Is.EqualTo(TestMetadata.IssueType.Id));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("Test issue"));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(256));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task UpdateIssueIgnoresUnchangingFields(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue",
                IntValue = 256
            }, CancellationToken.None);

            await jira.UpdateIssueAsync(reference, new JiraCustomIssue
            {
                Project = new JiraProject(),
                IssueType = new JiraIssueType(),
                Summary = "Test issue 1",
                IntValue = 257
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new[] {"project", "issueType", "summary", "customfield_43567"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Project, Is.Not.Null);
            Assert.That(response.Issues[0].Project.Id, Is.EqualTo(TestMetadata.Project.Id));
            Assert.That(response.Issues[0].IssueType, Is.Not.Null);
            Assert.That(response.Issues[0].IssueType.Id, Is.EqualTo(TestMetadata.IssueType.Id));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("Test issue 1"));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(257));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task CreatesMethodFillsOtherFields(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var now = DateTime.Now;
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue",
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new[] {"created", "updated", "creator"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Created, Is.GreaterThan(now));
            Assert.That(response.Issues[0].Updated, Is.GreaterThan(now));
            Assert.That(response.Issues[0].Creator, Is.Not.Null);
            Assert.That(response.Issues[0].Creator.Key, Is.EqualTo(TestMetadata.User.Key));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task UpdateFillsUpdated(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Updated = new DateTime(1900, 1, 1),
                Summary = "Test issue",
            }, CancellationToken.None);

            var now = DateTime.Now;

            await jira.UpdateIssueAsync(reference, new JiraCustomIssue(), CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new[] {"updated"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Updated, Is.GreaterThan(now));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task SelectReturnsOnlyRequiredFields(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue",
                IntValue = 1234
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY = {reference.Key}",
                Fields = new[] {"customfield_43567"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(1234));
            Assert.That(response.Issues[0].Summary, Is.Null);
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task SelectUsingStartAt(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference1 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue 1",
                IntValue = 1234
            }, CancellationToken.None);

            var reference2 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue 2",
                IntValue = 1235
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[43567]",
                StartAt = 1,
                Fields = new[] {"customfield_43567"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.StartAt, Is.EqualTo(1));
            Assert.That(response.Total, Is.EqualTo(2));
            Assert.That(response.MaxResults, Is.EqualTo(200));
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference2.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(1235));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public async Task SelectUsingMaxResult(JiraType type)
        {
            var jira = type == JiraType.File ? fileJira : memoryJira;
            var reference1 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue 1",
                IntValue = 1234
            }, CancellationToken.None);

            var reference2 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Summary = "Test issue 2",
                IntValue = 1235
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = "order by cf[43567]",
                MaxResults = 1,
                Fields = new[] {"customfield_43567"}
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.StartAt, Is.EqualTo(0));
            Assert.That(response.Total, Is.EqualTo(2));
            Assert.That(response.MaxResults, Is.EqualTo(1));
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference1.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(1234));
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(43567)]
            public int IntValue
            {
                get => CustomFields[43567].Get<int>();
                set => CustomFields[43567].Set(value);
            }

            private class Scope : IDefineScope<JiraCustomIssue>
            {
                public void Build(IScopeBuilder<JiraCustomIssue> builder)
                {
                    builder
                        .Define(x => x.Project, TestMetadata.Project)
                        .Define(x => x.IssueType, TestMetadata.IssueType);
                }
            }
        }
    }
}