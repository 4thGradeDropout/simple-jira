using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Impl.Queryable
{
    internal class RelinqQueryExecutor : IQueryExecutor
    {
        private readonly IJiraMetadataProvider metadataProvider;
        private readonly Func<BuiltQuery, IEnumerable> execute;

        public RelinqQueryExecutor(IJiraMetadataProvider metadataProvider, Func<BuiltQuery, IEnumerable> execute)
        {
            this.metadataProvider = metadataProvider;
            this.execute = execute;
        }

        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
        }

        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            var collection = ExecuteCollection<T>(queryModel);
            return returnDefaultWhenEmpty
                ? collection.SingleOrDefault()
                : collection.Single();
        }

        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var queryBuilder = new QueryBuilder();
            queryModel.Accept(new QueryModelVisitor(metadataProvider, queryBuilder, null));
            var builtQuery = queryBuilder.Build();
            return execute(builtQuery).Cast<T>();
        }
    }
}