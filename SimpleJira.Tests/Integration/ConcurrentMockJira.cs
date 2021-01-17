using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using NUnit.Framework;
using SimpleJira.Fakes.Interface;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration
{
    public class ConcurrentMockJira : TestBase
    {
        private IMockJira fileJira;
        private IMockJira inMemoryJira;

        protected override void SetUp()
        {
            base.SetUp();
            var metadataProvider = new JiraMetadataProvider(new Type[0]);
            var folderPath = Path.Combine(Path.GetTempPath(), "fileJiraStore");
            fileJira = FakeJira.File(folderPath, "http://fake.jira.int", new JiraUser(),
                metadataProvider);
            inMemoryJira = FakeJira.InMemory("http://fake.jira.int", new JiraUser(),
                metadataProvider);
            fileJira.Drop();
            inMemoryJira.Drop();
        }

        protected override void TearDown()
        {
            try
            {
                fileJira.Drop();
                inMemoryJira.Drop();
            }
            finally
            {
                base.TearDown();
            }
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public void Create(JiraType jiraType)
        {
            var threads = new Thread[10];
            Exception lastException = null;
            var lockObject = new object();
            int value = 0;
            var jira = jiraType == JiraType.File ? fileJira : inMemoryJira;
            for (int i = 0; i < threads.Length; i++)
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (int j = 0; j < 100; ++j)
                        {
                            jira.CreateIssue(new JiraCustomIssue
                            {
                                Project = TestMetadata.Project,
                                IssueType = TestMetadata.IssueType,
                                IntValue = Interlocked.Increment(ref value)
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });

            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();

            Assert.That(lastException, Is.Null);

            var response = jira.SelectIssues<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                MaxResults = 200,
                Jql = "project = TESTPROJECT",
            });
            Assert.That(response.Total, Is.EqualTo(1000));

            var issues = response.Issues.ToList();
            for (int i = 0; i < 4; ++i)
                issues.AddRange(jira.SelectIssues<JiraCustomIssue>(new JiraIssuesRequest
                {
                    StartAt = (i + 1) * 200,
                    MaxResults = 200,
                    Jql = "project = TESTPROJECT"
                }).Issues);

            Assert.That(issues.Count, Is.EqualTo(1000));
            Assert.That(issues.Select(x => x.IntValue),
                Is.EquivalentTo(Enumerable.Range(1, 1000)));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public void Update(JiraType jiraType)
        {
            var jira = jiraType == JiraType.File ? fileJira : inMemoryJira;
            var threads = new Thread[10];
            Exception lastException = null;
            var lockObject = new object();
            int value = 0;
            var reference = jira.CreateIssue(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = value
            });

            for (int i = 0; i < threads.Length; i++)
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (int j = 0; j < 100; ++j)
                        {
                            jira.UpdateIssue(reference, new JiraCustomIssue
                            {
                                Project = TestMetadata.Project,
                                IntValue = Interlocked.Increment(ref value)
                            });
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });

            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();

            Assert.That(lastException, Is.Null);

            var response = jira.SelectIssues<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                MaxResults = 200,
                Jql = "project = TESTPROJECT",
            });
            Assert.That(response.Total, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public void UploadAttachment(JiraType jiraType)
        {
            var jira = jiraType == JiraType.File ? fileJira : inMemoryJira;

            var threads = new Thread[10];
            Exception lastException = null;
            var lockObject = new object();
            int value = 0;
            var reference = jira.CreateIssue(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = value
            });

            for (int i = 0; i < threads.Length; i++)
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (int j = 0; j < 100; ++j)
                        {
                            jira.UploadAttachment(reference, $"{Interlocked.Increment(ref value)}.txt",
                                Encoding.UTF8.GetBytes("Hello, world!"));
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });

            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();

            Assert.That(lastException, Is.Null);

            var response = jira.SelectIssues<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                MaxResults = 200,
                Jql = "project = TESTPROJECT",
                Fields = new[] {"attachment"}
            });
            Assert.That(response.Total, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Attachment.Length, Is.EqualTo(1000));
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public void DownloadAttachment(JiraType jiraType)
        {
            var jira = jiraType == JiraType.File ? fileJira : inMemoryJira;
            var threads = new Thread[10];
            Exception lastException = null;
            var lockObject = new object();
            int value = 0;
            var reference = jira.CreateIssue(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = value
            });

            var attachment = jira.UploadAttachment(reference, $"{Interlocked.Increment(ref value)}.txt",
                Encoding.UTF8.GetBytes("Hello, world!"));


            for (int i = 0; i < threads.Length; i++)
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (int j = 0; j < 100; ++j)
                        {
                            var bytes = jira.DownloadAttachment(attachment);
                            Assert.That(Encoding.UTF8.GetString(bytes), Is.EqualTo("Hello, world!"));
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });

            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();

            Assert.That(lastException, Is.Null);
        }

        [TestCase(JiraType.File)]
        [TestCase(JiraType.InMemory)]
        public void DeleteAttachment(JiraType jiraType)
        {
            var jira = jiraType == JiraType.File ? fileJira : inMemoryJira;
            var threads = new Thread[10];
            Exception lastException = null;
            var lockObject = new object();
            var value = 0;
            var reference = jira.CreateIssue(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = value
            });
            var attachments = new JiraAttachment[1000];
            for (var i = 0; i < 10; ++i)
            {
                var threadIndex = i;
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (var j = 0; j < 100; ++j)
                        {
                            attachments[j * 10 + threadIndex] = jira.UploadAttachment(reference,
                                $"{Interlocked.Increment(ref value)}.txt",
                                Encoding.UTF8.GetBytes("Hello, world!"));
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });
            }

            foreach (var t in threads)
                t.Start();
            foreach (var t in threads)
                t.Join();

            Assert.That(lastException, Is.Null);
            int count = 0;

            for (int i = 0; i < threads.Length; i++)
            {
                var threadIndex = i;
                threads[i] = new Thread(o =>
                {
                    try
                    {
                        for (var j = 0; j < 100; ++j)
                        {
                            var attachment = attachments[j * 10 + threadIndex];
                            jira.DeleteAttachment(attachment);
                            Interlocked.Increment(ref count);
                        }
                    }
                    catch (Exception e)
                    {
                        lock (lockObject) lastException = e;
                    }
                });
            }

            foreach (var t in threads)
                t.Start();

            foreach (var t in threads)
                t.Join();


            Assert.That(lastException, Is.Null);
            Assert.That(count, Is.EqualTo(1000));

            var response = jira.SelectIssues<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                MaxResults = 200,
                Jql = "project = TESTPROJECT",
                Fields = new[] {"attachment"}
            });
            Assert.That(response.Total, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(reference.Key));
            Assert.That(response.Issues[0].Attachment.Length, Is.EqualTo(0));
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(54512)]
            public int IntValue
            {
                get => CustomFields[54512].Get<int>();
                set => CustomFields[54512].Set(value);
            }
        }
    }
}