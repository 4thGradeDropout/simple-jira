using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;

namespace SimpleJira.Impl.Queryable
{
    internal class RelinqQueryable<T> : QueryableBase<T>
    {
        public RelinqQueryable(IQueryProvider queryProvider)
            : base(queryProvider)
        {
        }

        public RelinqQueryable(IQueryProvider queryProvider, Expression expression)
            : base(queryProvider, expression)
        {
        }
    }
}