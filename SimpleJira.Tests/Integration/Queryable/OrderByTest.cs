using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class OrderByTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task Test()
        {
            var issue15 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
                IntValue = 15
            }, CancellationToken.None);
            var issue10 = await jira.CreateIssueAsync(new JiraCustomIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
                IntValue = 10
            }, CancellationToken.None);

            var issues = Source<JiraCustomIssue>().OrderBy(x => x.IntValue).ToArray();
            Assert.That(issues.Length, Is.EqualTo(2));
            Assert.That(issues[0].Key, Is.EqualTo(issue10.Key));
            Assert.That(issues[1].Key, Is.EqualTo(issue15.Key));
        }

        private static JiraProject TestProject => new JiraProject
        {
            Id = "42152",
            Key = "TESTPROJECT",
            Name = "Тестовый проект",
            Self = "https://jira.knopka.com/rest/api/2/project/42152",
            AvatarUrls = new JiraAvatarUrls(),
            ProjectTypeKey = "business",
        };

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
    }
}