using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class AnyTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task Test()
        {
            for (var i = 0; i < 1643; ++i)
                await jira.CreateIssueAsync(new JiraCustomIssue
                {
                    Project = TestProject,
                    IssueType = TestMetadata.IssueType,
                    Summary = "Тестовая тема"
                }, CancellationToken.None);
            Assert.That(
                Source<JiraCustomIssue>().Any(x => JqlFunctions.Contains(x.Summary, "тема")),
                Is.True);
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
        }
    }
}