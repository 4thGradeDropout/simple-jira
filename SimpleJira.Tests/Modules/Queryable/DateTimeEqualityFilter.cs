using System;
using System.Linq;
using NUnit.Framework;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class DateTimeEqualityFilter : QueryBuilderTest
    {
        [Test]
        public void Date()
        {
            AssertQuery(Source<JiraIssue>().Where(x => x.Created == new DateTime(2020, 8, 13)),
                "(created = '2020-08-13')");
        }

        [Test]
        public void DateTime()
        {
            AssertQuery(Source<JiraIssue>().Where(x => x.Created == new DateTime(2020, 8, 13, 6, 58, 0)),
                "(created = '2020-08-13 06:58')");
        }
    }
}