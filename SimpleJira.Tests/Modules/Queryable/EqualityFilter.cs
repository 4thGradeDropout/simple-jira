using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class EqualityFilter : QueryBuilderTest
    {
        [Test]
        public void String()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.StringValue == "test"),
                "(CF[12345] = \"test\")");
        }

        [Test]
        public void Number()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.NumberValue == 12345),
                "(CF[12346] = 12345)");
        }

        [Test]
        public void FieldIsAlwaysOnTheLeft()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => "test" == x.StringValue),
                "(CF[12345] = \"test\")");
        }
        
        [Test]
        public void CustomField_Int()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.CustomFields[12345].Get<string>() == "test"),
                "(CF[12345] = \"test\")");
        }
        
        [Test]
        public void CustomField_String()
        {
            AssertQuery(Source<JiraCustomIssue>()
                    .Where(x => x.CustomFields["12345"].Get<string>() == "test"),
                "(CF[12345] = \"test\")");
        }

        private class JiraCustomIssue : JiraIssue
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
            public int NumberValue
            {
                get => CustomFields["12346"].Get<int>();
                set => CustomFields["12346"].Set(value);
            }
        }
    }
}