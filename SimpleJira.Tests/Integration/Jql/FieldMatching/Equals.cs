using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Jql.FieldMatching
{
    public class Equals : FieldMatchingTestBase
    {
        [Test]
        public async Task ByKey()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                StringValue = "some_string_value"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                Jql = $"KEY={issue.Key}",
                StartAt = 0,
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].StringValue, Is.EqualTo("some_string_value"));
        }

        [Test]
        public async Task ByString()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "some_string_value"
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "summary = some_string_value",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Summary, Is.EqualTo("some_string_value"));
        }

        [Test]
        public async Task ByInt()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 6784
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12346] = 6784",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task ByLong()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                LongValue = 6784
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12347] = 6784",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].LongValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task ByDecimal()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DecimalValue = 6784.56m
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12348] = 6784.56",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DecimalValue, Is.EqualTo(6784.56m));
        }

        [Test]
        public async Task ByDate_Format1()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 12)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] = '2020/8/12'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 12)));
        }

        [Test]
        public async Task ByDate_Format2()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 12)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] = '2020-08-12'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 12)));
        }

        [Test]
        public async Task ByDateTime_Format1()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 12, 23, 49, 0)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] = '2020/8/12 23:49'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 12, 23, 49, 0)));
        }

        [Test]
        public async Task ByDateTime_Format2()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 12, 23, 49, 0)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] = '2020-08-12 23:49'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 12, 23, 49, 0)));
        }

        [Test]
        public async Task Status_Name()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Status = TestMetadata.Status
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"status = \"{TestMetadata.Status.Name}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Status.Id,
                Is.EqualTo(TestMetadata.Status.Id));
        }

        [Test]
        public async Task Status_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Status = TestMetadata.Status
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"status = \"{TestMetadata.Status.Id}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Status.Id,
                Is.EqualTo(TestMetadata.Status.Id));
        }

        [Test]
        public async Task CustomFieldOption_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                CustomField = TestMetadata.CustomFieldOption
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"cf[12350] = {TestMetadata.CustomFieldOption.Id}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].CustomField.Id,
                Is.EqualTo(TestMetadata.CustomFieldOption.Id));
        }

        [Test]
        public async Task CustomFieldOption_Value()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                CustomField = TestMetadata.CustomFieldOption
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"cf[12350] = \"{TestMetadata.CustomFieldOption.Value}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].CustomField.Id,
                Is.EqualTo(TestMetadata.CustomFieldOption.Id));
        }

        [Test]
        public async Task Priority_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Priority = TestMetadata.Priority
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"priority = {TestMetadata.Priority.Id}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Priority.Id,
                Is.EqualTo(TestMetadata.Priority.Id));
        }

        [Test]
        public async Task Priority_Name()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Priority = TestMetadata.Priority
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"priority = \"{TestMetadata.Priority.Name}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Priority.Id,
                Is.EqualTo(TestMetadata.Priority.Id));
        }

        [Test]
        public async Task Project_Key()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"project = {TestMetadata.Project.Key}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Project.Id,
                Is.EqualTo(TestMetadata.Project.Id));
        }

        [Test]
        public async Task Project_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"project = {TestMetadata.Project.Id}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Project.Id,
                Is.EqualTo(TestMetadata.Project.Id));
        }

        [Test]
        public async Task Project_Name()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"project = \"{TestMetadata.Project.Name}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Project.Id,
                Is.EqualTo(TestMetadata.Project.Id));
        }

        [Test]
        public async Task User_Key()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Assignee = TestMetadata.User
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"assignee = \"{TestMetadata.User.Key}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Assignee.Key,
                Is.EqualTo(TestMetadata.User.Key));
        }

        [Test]
        public async Task User_Name()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Assignee = TestMetadata.User
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"assignee = \"{TestMetadata.User.Name}\"",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Assignee.Key,
                Is.EqualTo(TestMetadata.User.Key));
        }

        [Test]
        public async Task IssueReference_Key()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Parent = TestMetadata.Reference,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"parent = {TestMetadata.Reference.Key}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Parent.Key,
                Is.EqualTo(TestMetadata.Reference.Key));
        }

        [Test]
        public async Task IssueReference_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Parent = TestMetadata.Reference,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"parent = {TestMetadata.Reference.Id}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].Parent.Key,
                Is.EqualTo(TestMetadata.Reference.Key));
        }

        [Test]
        public async Task IssueType_Id()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"issuetype = {TestMetadata.IssueType.Id}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].IssueType.Id,
                Is.EqualTo(TestMetadata.IssueType.Id));
        }

        [Test]
        public async Task IssueType_Name()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = $"issuetype = {TestMetadata.IssueType.Name}",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].IssueType.Id,
                Is.EqualTo(TestMetadata.IssueType.Id));
        }

        [Test]
        public async Task Array()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Labels = new[] {"label1", "label2"}
            }, CancellationToken.None);

            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "labels = label1",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
        }
    }
}