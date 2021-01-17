using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class BinaryFilter : QueryBuilderTest
    {
        [Test]
        public void And()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => JqlFunctions.Contains(x.Summary, "test") && x.Parent == "SOME_KEY"),
                "((summary ~ \"test\") AND (parent = \"SOME_KEY\"))");
        }

        [Test]
        public void Or()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => JqlFunctions.Contains(x.Summary, "test") || x.Parent == "SOME_KEY"),
                "((summary ~ \"test\") OR (parent = \"SOME_KEY\"))");
        }
    }
}