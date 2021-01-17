using System.Collections;
using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class InFilter : QueryBuilderTest
    {
        //TODO разобраться с Labels NOT IN
        [Test]
        public void Positive()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => new[] {Status1, Status2}.Contains(x.Status)),
                "(status IN (\"1\", \"2\"))");
        }

        [Test]
        public void Negative()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => !new[] {Status1, Status2}.Contains(x.Status)),
                "(NOT (status IN (\"1\", \"2\")))");
        }

        private static JiraStatus Status1 => new JiraStatus
        {
            Id = "1",
            Name = "Открыта",
            StatusCategory = new JiraStatusCategory
            {
                Key = "new",
                Id = 2,
                Name = "К выполнению",
                Self = "https://jira.knopka.com/rest/api/2/statuscategory/2",
                ColorName = "blue-gray"
            },
            Self = "https://jira.knopka.com/rest/api/2/status/1",
            Description = "Создана новая задача",
            IconUrl = "https://jira.knopka.com/images/icons/statuses/open.png"
        };

        private static JiraStatus Status2 => new JiraStatus
        {
            Id = "2",
            Name = "Готово",
            StatusCategory = new JiraStatusCategory
            {
                Key = "done",
                Id = 3,
                Name = "Готово",
                Self = "https://jira.knopka.com/rest/api/2/statuscategory/3",
                ColorName = "blue-gray"
            },
            Self = "https://jira.knopka.com/rest/api/2/status/2",
            Description = "Готово",
            IconUrl = "https://jira.knopka.com/images/icons/statuses/done.png"
        };
    }
}