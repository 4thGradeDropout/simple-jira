using System.Linq;
using NUnit.Framework;
using SimpleJira.Impl.Queryable;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Modules.Queryable
{
    public class ProjectionTest : TestBase
    {
        [Test]
        public void Projection()
        {
            AssertQuery(Source<JiraIssue>()
                .Where(x => x.Status == "1")
                .Select(x => new
                {
                    x.Parent.Key
                }), "(status = \"1\")", new[] {"parent"});
        }

        private BuiltQuery lastQuery;

        private IQueryable<T> Source<T>()
        {
            var queryProvider = RelinqHelpers.CreateQueryProvider(new JiraMetadataProvider(new[] {typeof(T)}),
                delegate(BuiltQuery query)
                {
                    lastQuery = query;
                    return new T[0];
                });
            return new RelinqQueryable<T>(queryProvider);
        }

        private void AssertQuery<T>(IQueryable<T> query, string expectedQueryText, string[] fields)
        {
            lastQuery = null;
            Assert.That(query.ToArray().Length, Is.EqualTo(0));
            Assert.IsNotNull(lastQuery);
            Assert.That(lastQuery.Query.ToString(), Is.EqualTo(expectedQueryText));
            if (fields == null)
                Assert.That(lastQuery.Projection, Is.Null);
            else
                Assert.That(lastQuery.Projection.fields.Select(x => x.Expression).ToArray(),
                    Is.EquivalentTo(fields));
        }
    }
}