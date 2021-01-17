using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class StatusEqualityFilter : QueryBuilderTest
    {
        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.Status == JiraCustomIssue.Metadata.Открыта),
                "(status = \"1\")");
        }

        [Test]
        public void Name()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.Status == "Открыта"),
                "(status = \"Открыта\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.Status.Name == "Открыта"),
                "(status = \"Открыта\")");
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            public static class Metadata
            {
                public static JiraStatus Открыта => new JiraStatus
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
            }
        }
    }
}