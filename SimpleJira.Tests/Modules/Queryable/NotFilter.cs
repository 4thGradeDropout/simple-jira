using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class NotFilter : QueryBuilderTest
    {
        [Test]
        public void Test()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => !(x.Key == "TESTKEY-12345")),
                "(NOT (key = \"TESTKEY-12345\"))");
        }
    }
}