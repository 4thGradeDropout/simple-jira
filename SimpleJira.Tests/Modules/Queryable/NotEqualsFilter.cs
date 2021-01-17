using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class NotEqualsFilter : QueryBuilderTest
    {
        [Test]
        public void Simple()
        {
            AssertQuery(Source<JiraIssue>().Where(x => x.Summary != "xxx"),
                "(summary != \"xxx\")");
        }
    }
}