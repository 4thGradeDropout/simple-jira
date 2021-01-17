using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class UserEqualityFilter : QueryBuilderTest
    {
        private static JiraUser I_Medvedev => new JiraUser
        {
            Key = "SOME_KEY",
            Name = "i.medvedev",
            Active = true,
            DisplayName = "Ivan Medvedev"
        };

        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Assignee == I_Medvedev),
                "(assignee = \"SOME_KEY\")");
        }

        [Test]
        public void Name()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Assignee == "i.medvedev"),
                "(assignee = \"i.medvedev\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Assignee.Name == "i.medvedev"),
                "(assignee = \"i.medvedev\")");
        }
    }
}