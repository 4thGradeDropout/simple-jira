using System;
using System.Linq.Expressions;

namespace SimpleJira.Impl.Queryable
{
    internal class SelectedProperty
    {
        public Expression expression;
        public Func<object[], object> compiledExpression;
        public bool needLocalEval;
        public SelectedPropertyItem[] items;
        public bool isReference;
    }
}