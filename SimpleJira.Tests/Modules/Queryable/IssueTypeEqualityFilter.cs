using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class IssueTypeEqualityFilter : QueryBuilderTest
    {
        private static JiraIssueType Question => new JiraIssueType
        {
            Description = "Вопрос от клиента",
            IconUrl = "https://jira.knopka.com/images/icons/issuetypes/undefined.png",
            Id = "32",
            Name = "Вопрос от клиента",
            Self = "https://jira.knopka.com/rest/api/2/issuetype/32",
            SubTask = true
        };

        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.IssueType == Question),
                "(issuetype = \"32\")");
        }

        [Test]
        public void Name()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.IssueType == "Вопрос от клиента"),
                "(issuetype = \"Вопрос от клиента\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.IssueType.Name == "Вопрос от клиента"),
                "(issuetype = \"Вопрос от клиента\")");
        }

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