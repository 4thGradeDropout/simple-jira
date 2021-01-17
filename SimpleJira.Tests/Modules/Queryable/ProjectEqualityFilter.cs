using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class ProjectEqualityFilter : QueryBuilderTest
    {
        private static JiraProject Question => new JiraProject
        {
            Id = "1",
            Key = "KNOPKLIENT",
            Name = "Some Question"
        };

        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Project == Question),
                "(project = \"KNOPKLIENT\")");
        }

        [Test]
        public void Name()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Project == "Some Question"),
                "(project = \"Some Question\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Project.Key == "KNOPKLIENT"),
                "(project = \"KNOPKLIENT\")");
        }
    }
}