using System;
using SimpleJira.Fakes.Interface;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Jql.FieldMatching
{
    public abstract class FieldMatchingTestBase : TestBase
    {
        protected static IJira CreateJira()
        {
            return FakeJira.InMemory("http://fake.jira.int", TestMetadata.User,
                new JiraMetadataProvider(new[] {typeof(JiraCustomIssue)}));
        }

        protected class JiraCustomIssue : JiraIssue
        {
            public JiraCustomIssue()
            {
            }

            public JiraCustomIssue(IJiraIssueFieldsController controller) : base(controller)
            {
            }

            [JiraIssueProperty(12345)]
            public string StringValue
            {
                get => CustomFields["12345"].Get<string>();
                set => CustomFields["12345"].Set(value);
            }

            [JiraIssueProperty(12346)]
            public int IntValue
            {
                get => CustomFields["12346"].Get<int>();
                set => CustomFields["12346"].Set(value);
            }

            [JiraIssueProperty(12347)]
            public long LongValue
            {
                get => CustomFields["12347"].Get<long>();
                set => CustomFields["12347"].Set(value);
            }

            [JiraIssueProperty(12348)]
            public decimal DecimalValue
            {
                get => CustomFields["12348"].Get<decimal>();
                set => CustomFields["12348"].Set(value);
            }

            [JiraIssueProperty(12349)]
            public DateTime DateTimeValue
            {
                get => CustomFields["12349"].Get<DateTime>();
                set => CustomFields["12349"].Set(value);
            }

            [JiraIssueProperty(12350)]
            public JiraCustomFieldOption CustomField
            {
                get => CustomFields["12350"].Get<JiraCustomFieldOption>();
                set => CustomFields["12350"].Set(value);
            }
        }
    }
}