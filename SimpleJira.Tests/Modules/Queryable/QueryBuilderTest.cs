using System.Linq;
using NUnit.Framework;
using SimpleJira.Impl.Queryable;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Tests.Modules.Queryable
{
    public abstract class QueryBuilderTest : TestBase
    {
        private BuiltQuery lastQuery;

        protected IQueryable<T> Source<T>()
        {
            var queryProvider = RelinqHelpers.CreateQueryProvider(new JiraMetadataProvider(new[] {typeof(T)}),
                delegate(BuiltQuery query)
                {
                    lastQuery = query;
                    return new T[0];
                });
            return new RelinqQueryable<T>(queryProvider);
        }

        protected void AssertQuery<T>(IQueryable<T> query, string expectedQueryText)
        {
            lastQuery = null;
            Assert.That(query.ToArray().Length, Is.EqualTo(0));
            Assert.That(lastQuery, Is.Not.Null);
            Assert.That(lastQuery.Query.ToString(), Is.EqualTo(expectedQueryText));
        }
    }
}