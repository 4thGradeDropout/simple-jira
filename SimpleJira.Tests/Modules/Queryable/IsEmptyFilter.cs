using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class IsEmptyFilter : QueryBuilderTest
    {
        [Test]
        public void Positive()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.StringValue == null),
                "(CF[14356] IS EMPTY)");
        }

        [Test]
        public void Negative()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.StringValue != null),
                "(CF[14356] IS NOT EMPTY)");
        }

        private class JiraCustomIssue : JiraIssue
        {
            [JiraIssueProperty(14356)]
            public string StringValue
            {
                get => CustomFields["14356"].Get<string>();
                set => CustomFields["14356"].Set(value);
            }
        }
    }
}