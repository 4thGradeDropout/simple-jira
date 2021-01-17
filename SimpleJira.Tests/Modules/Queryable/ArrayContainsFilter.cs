using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class ArrayContainsFilter : QueryBuilderTest
    {
        [Test]
        public void IsEmpty()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Labels == null),
                "(labels IS EMPTY)");
        }

        [Test]
        public void Contains()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Labels.Contains("lab1")),
                "(labels = \"lab1\")");
        }

        [Test]
        public void Complex()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Labels == null || x.Labels.Contains("lab1")),
                "((labels IS EMPTY) OR (labels = \"lab1\"))");
        }
    }
}