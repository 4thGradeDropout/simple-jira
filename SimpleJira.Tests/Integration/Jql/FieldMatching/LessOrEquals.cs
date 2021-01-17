using System;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Jql.FieldMatching
{
    public class LessOrEquals : FieldMatchingTestBase
    {
        [Test]
        public async Task Int_Less()
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
                Jql = "cf[12346] <= 6785",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task Int_Equals()
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
                Jql = "cf[12346] <= 6784",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].IntValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task Int_Creater()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 6784
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12346] <= 6783",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Long_Less()
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
                Jql = "cf[12347] <= 6785",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].LongValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task Long_Equals()
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
                Jql = "cf[12347] <= 6784",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].LongValue, Is.EqualTo(6784));
        }

        [Test]
        public async Task Long_Greater()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                LongValue = 6784
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12347] <= 6783",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }


        [Test]
        public async Task Decimal_Less()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DecimalValue = 6784.12m
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12348] <= 6784.13",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DecimalValue, Is.EqualTo(6784.12m));
        }

        [Test]
        public async Task Decimal_Equals()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DecimalValue = 6784.12m
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12348] <= 6784.12",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DecimalValue, Is.EqualTo(6784.12m));
        }

        [Test]
        public async Task Decimal_Greater()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DecimalValue = 6784.12m
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12348] <= 6784.11",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Date_Format1_Less()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020/8/21'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 20)));
        }

        [Test]
        public async Task Date_Format1_Equals()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020/8/20'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 20)));
        }

        [Test]
        public async Task Date_Format1_Greater()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020/8/19'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }

        [Test]
        public async Task Date_Format2_Less()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020-08-21'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 20)));
        }

        [Test]
        public async Task Date_Format2_Equals()
        {
            var jira = CreateJira();
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020-08-20'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(1));
            Assert.That(response.Issues[0].Key, Is.EqualTo(issue.Key));
            Assert.That(response.Issues[0].DateTimeValue,
                Is.EqualTo(new DateTime(2020, 8, 20)));
        }

        [Test]
        public async Task Date_Format2_Greater()
        {
            var jira = CreateJira();
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                DateTimeValue = new DateTime(2020, 8, 20)
            }, CancellationToken.None);
            var response = await jira.SelectIssuesAsync<JiraCustomIssue>(new JiraIssuesRequest
            {
                StartAt = 0,
                Jql = "cf[12349] <= '2020-08-19'",
                MaxResults = 5000
            });

            Assert.That(response.Issues.Length, Is.EqualTo(0));
        }
    }
}