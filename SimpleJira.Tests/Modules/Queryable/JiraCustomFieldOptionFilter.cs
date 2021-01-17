using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class JiraCustomFieldOptionFilter : QueryBuilderTest
    {
        [Test]
        public void Strict()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.HasDeadline == JiraCustomIssue.Metadata.HasDeadline.Yes),
                "(CF[16563] = \"19313\")");
        }

        [Test]
        public void Value()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.HasDeadline == "Yes"),
                "(CF[16563] = \"Yes\")");
        }

        [Test]
        public void SubProperty()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.HasDeadline.Value == "Yes"),
                "(CF[16563] = \"Yes\")");
        }

        private class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(16563)]
            public JiraCustomFieldOption HasDeadline
            {
                get => CustomFields["16563"].Get<JiraCustomFieldOption>();
                set => CustomFields["16563"].Set(value);
            }

            public static class Metadata
            {
                public static class HasDeadline
                {
                    public static JiraCustomFieldOption Yes => new JiraCustomFieldOption
                    {
                        Id = "19313",
                        Self = "https://jira.knopka.com/rest/api/2/customFieldOption/19313",
                        Value = "Yes"
                    };
                }
            }
        }
    }
}