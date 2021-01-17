using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class IssueFunctionFilter : QueryBuilderTest
    {
        [Test]
        public void SubTasksOf()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => Source<JiraIssue>()
                        .Where(y => JqlFunctions.Contains(y.Summary, "test"))
                        .Any(y => y == x.Parent)),
                "(ISSUEFUNCTION IN SUBTASKSOF(\"(summary ~ \\\"test\\\")\"))");
        }

        [Test]
        public void SubTasksOf_Complex()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Key == "TEST_KEY")
                    .Where(x => Source<JiraIssue>()
                        .Where(y => JqlFunctions.Contains(y.Summary, "test"))
                        .Any(y => y == x.Parent)),
                "((key = \"TEST_KEY\") AND (ISSUEFUNCTION IN SUBTASKSOF(\"(summary ~ \\\"test\\\")\")))");
        }

        [Test]
        public void ParentsOf()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => Source<JiraIssue>()
                        .Where(y => JqlFunctions.Contains(y.Summary, "test"))
                        .Any(y => y.Parent == x)),
                "(ISSUEFUNCTION IN PARENTSOF(\"(summary ~ \\\"test\\\")\"))");
        }

        [Test]
        public void ParentsOf_Complex()
        {
            AssertQuery(Source<JiraIssue>()
                    .Where(x => x.Key == "TEST_KEY")
                    .Where(x => Source<JiraIssue>()
                        .Where(y => JqlFunctions.Contains(y.Summary, "test"))
                        .Any(y => y.Parent == x)),
                "((key = \"TEST_KEY\") AND (ISSUEFUNCTION IN PARENTSOF(\"(summary ~ \\\"test\\\")\")))");
        }
    }
}