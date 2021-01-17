using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class WhereFilter : QueryBuilderTest
    {
        [Test]
        public void Two()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => JqlFunctions.Contains(x.Summary, "test"))
                    .Where(x => x.Parent == "SOME_KEY"),
                "((summary ~ \"test\") AND (parent = \"SOME_KEY\"))");
        }
    }
}