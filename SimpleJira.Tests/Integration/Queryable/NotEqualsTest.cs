using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Queryable
{
    public class NotEqualsTest : QueryableTestBase
    {
        private protected override IEnumerable<Type> GetIssueTypes()
        {
            return new[] {typeof(JiraCustomIssue)};
        }

        [Test]
        public async Task ByKey_False()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            AssertEmpty(Source<JiraCustomIssue>().Where(x => x.Key != reference.Key));
        }

        [Test]
        public async Task ByKey_True()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            AssertSingle(Source<JiraCustomIssue>().Where(x => x.Key != Guid.NewGuid().ToString()), reference);
        }

        [Test]
        public async Task ById_False()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            AssertEmpty(Source<JiraCustomIssue>().Where(x => x.Id != reference.Id));
        }

        [Test]
        public async Task ById_True()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);
            AssertSingle(Source<JiraCustomIssue>().Where(x => x.Id != Guid.NewGuid().ToString()), reference);
        }

        [Test]
        public async Task ByStatusCategory_False()
        {
            await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
                Status = TestStatus
            }, CancellationToken.None);
            AssertEmpty(Source<JiraCustomIssue>().Where(x => x.Status.StatusCategory != TestStatusCategory));
        }

        [Test]
        public async Task ByStatusCategory_True()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
                Status = TestStatus
            }, CancellationToken.None);
            AssertSingle(Source<JiraCustomIssue>().Where(x => x.Status.StatusCategory != Guid.NewGuid().ToString()),
                reference);
        }

        [Test]
        public async Task ByAnotherField_False()
        {
            await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            AssertEmpty(Source<JiraCustomIssue>().Where(x => x.Project != TestProject));
        }

        [Test]
        public async Task ByAnotherField_True()
        {
            var reference = await jira.CreateIssueAsync(new JiraIssue
            {
                Project = TestProject,
                IssueType = TestMetadata.IssueType,
            }, CancellationToken.None);

            AssertSingle(Source<JiraCustomIssue>().Where(x => x.Project != Guid.NewGuid().ToString()), reference);
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

        private static JiraStatusCategory TestStatusCategory => new JiraStatusCategory
        {
            Key = "test_status_category",
            Id = 1,
            Name = "Test Status Category",
            Self = "https://jira.int/rest/api/2/statuscategory/1",
            ColorName = "medium-gray"
        };

        private static JiraStatus TestStatus => new JiraStatus
        {
            Id = "6",
            Name = "Test Status",
            StatusCategory = TestStatusCategory,
            Self = "https://jira.int/rest/api/2/status/6",
            Description = "",
            IconUrl = "https://jira.int/images/icons/statuses/closed.png"
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