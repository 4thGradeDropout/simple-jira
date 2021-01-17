using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class ProjectionTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task Anonymous_Default()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new {x.IntValue})
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
        }

        [Test]
        public async Task Anonymous_NamedProperty()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new {Value = x.IntValue})
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Value, Is.EqualTo(15));
        }

        [Test]
        public async Task Typed()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new Projection {Value = x.IntValue})
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Value, Is.EqualTo(15));
        }

        [Test]
        public async Task ConstructorInitialization()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new AwesomeType(x.IntValue))
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
        }

        [Test]
        public async Task ConstructorInitialization_Mix()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15,
                Summary = "some summary"
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new AwesomeType(x.IntValue)
                {
                    Summary = x.Summary
                })
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
            Assert.That(issues[0].Summary, Is.EqualTo("some summary"));
        }

        [Test]
        public async Task PropertyOnly()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => x.IntValue)
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(15));
        }

        [Test]
        public async Task Anonymous_Reference()
        {
            var issue = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new {Reference = x.Reference()})
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Reference.Key, Is.EqualTo(issue.Key));
            Assert.That(issues[0].Reference.Id, Is.EqualTo(issue.Id));
            Assert.That(issues[0].Reference.Self, Is.EqualTo(issue.Self));
        }

        [Test]
        public async Task Reference()
        {
            var reference = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var references = provider.GetIssues<JiraCustomIssue>()
                .Select(x => x.Reference())
                .ToArray();
            Assert.That(references.Length, Is.EqualTo(1));
            Assert.That(references[0].Key, Is.EqualTo(reference.Key));
            Assert.That(references[0].Id, Is.EqualTo(reference.Id));
            Assert.That(references[0].Self, Is.EqualTo(reference.Self));
        }

        [Test]
        public async Task Anonymous_CustomField()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraIssue>()
                .Select(x => new {IntValue = x.CustomFields[12345].Get<int>()})
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
        }

        [Test]
        public async Task CustomField()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraIssue>()
                .Select(x => x.CustomFields[12345].Get<int>())
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(15));
        }

        [Test]
        public async Task LocalEvaluation()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => x.IntValue + x.IntValue)
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(30));
        }

        [Test]
        public async Task LocalEvaluation_LocalFunction()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => LocalFunction(x.IntValue))
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(LocalFunction(15)));
        }

        [Test]
        public async Task LocalEvaluation_SubProperty()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => ToUpperCase(x.IssueType.Name))
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(ToUpperCase(TestMetadata.IssueType.Name)));
        }

        [Test]
        public async Task LocalEvaluation_Constant()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => x.IntValue + 1)
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(16));
        }

        [Test]
        public async Task LocalEvaluation_CustomField()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => x.CustomFields[12345].Get<int>() + x.CustomFields[12345].Get<int>())
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0], Is.EqualTo(30));
        }

        [Test]
        public async Task SeveralProperties()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test",
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new
                {
                    x.Summary,
                    x.IntValue
                })
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Summary, Is.EqualTo("Test"));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
        }

        [Test]
        public async Task PropertyChain()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test",
                IntValue = 15
            }, CancellationToken.None);
            var issues = provider.GetIssues<JiraCustomIssue>()
                .Select(x => new
                {
                    x.Project.Key,
                })
                .ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Key, Is.EqualTo(TestMetadata.Project.Key));
        }

        [Test]
        public async Task TwoIssues()
        {
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test 1",
                IntValue = 15
            }, CancellationToken.None);
            await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestMetadata.Project,
                IssueType = TestMetadata.IssueType,
                Summary = "Test 2",
                IntValue = 16
            }, CancellationToken.None);

            var issues = provider.GetIssues<JiraCustomIssue>()
                .OrderBy(x => x.IntValue)
                .Select(x => new {x.IntValue, x.Summary})
                .ToArray();

            Assert.That(issues.Length, Is.EqualTo(2));
            Assert.That(issues[0].IntValue, Is.EqualTo(15));
            Assert.That(issues[0].Summary, Is.EqualTo("Test 1"));
            Assert.That(issues[1].IntValue, Is.EqualTo(16));
            Assert.That(issues[1].Summary, Is.EqualTo("Test 2"));
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(12345)]
            public int IntValue
            {
                get => CustomFields["12345"].Get<int>();
                set => CustomFields["12345"].Set(value);
            }
        }

        private class Projection
        {
            public int Value { get; set; }
        }

        private static string LocalFunction(int value) => value.ToString();

        private static string ToUpperCase(string value) => value?.ToUpper();

        private class AwesomeType
        {
            public AwesomeType(int value)
            {
                IntValue = value;
            }

            public int IntValue { get; }

            public string Summary { get; set; }
        }
    }
}