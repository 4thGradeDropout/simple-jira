using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class CompareFilter : QueryBuilderTest
    {
        [Test]
        public void Greater()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.NumberValue > 12),
                "(CF[12346] > 12)");
        }

        [Test]
        public void GreaterOrEqualTo()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.NumberValue >= 12),
                "(CF[12346] >= 12)");
        }

        [Test]
        public void Less()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.NumberValue < 12),
                "(CF[12346] < 12)");
        }

        [Test]
        public void LessOrEqualTo()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.NumberValue <= 12),
                "(CF[12346] <= 12)");
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(12346)]
            public int NumberValue
            {
                get => CustomFields["12346"].Get<int>();
                set => CustomFields["12346"].Set(value);
            }
        }
    }
}