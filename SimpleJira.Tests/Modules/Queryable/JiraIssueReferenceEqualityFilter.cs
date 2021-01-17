using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class JiraIssueReferenceEqualityFilter : QueryBuilderTest
    {
        [Test]
        public void Strict()
        {
            var reference = new JiraIssueReference
            {
                Key = "SOME_KEY",
                Id = "SOME_ID",
            };

            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Parent == reference),
                "(parent = \"SOME_KEY\")");
        }

        [Test]
        public void Key()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Parent == "SOME_KEY"),
                "(parent = \"SOME_KEY\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Parent.Key == "SOME_KEY"),
                "(parent = \"SOME_KEY\")");
        }
    }
}