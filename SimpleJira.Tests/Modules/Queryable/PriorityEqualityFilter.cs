using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class PriorityEqualityFilter : QueryBuilderTest
    {
        private static JiraPriority Critical => new JiraPriority
        {
            Name = "Critical",
            Id = "23"
        };

        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Priority == Critical),
                "(priority = \"23\")");
        }

        [Test]
        public void Name()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Priority == "Critical"),
                "(priority = \"Critical\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Priority.Name == "Critical"),
                "(priority = \"Critical\")");
        }
    }
}