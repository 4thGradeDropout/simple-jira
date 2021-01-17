using System.Collections.Generic;

namespace SimpleJira.Impl.Ast
{
    internal abstract class JqlVisitor
    {
        public virtual IJqlClause Visit(IJqlClause clause)
        {
            return clause.Accept(this);
        }

        public virtual IJqlClause VisitValue(ValueExpression clause)
        {
            return clause;
        }

        public virtual IJqlClause VisitFieldReference(FieldReferenceExpression clause)
        {
            return clause;
        }

        public virtual IJqlClause VisitFieldMatching(FieldMatchingExpression clause)
        {
            var field = Visit(clause.Field);
            var value = Visit(clause.Value);
            if (field == clause.Field && value == clause.Value)
                return clause;
            return new FieldMatchingExpression
            {
                Field = field,
                Value = value,
                Operator = clause.Operator
            };
        }

        public virtual IJqlClause VisitOrderBy(OrderByExpression clause)
        {
            var changed = false;
            var fieldOrderings = new FieldOrdering[clause.Fields.Length];
            for (var i = 0; i < clause.Fields.Length; ++i)
            {
                var field = clause.Fields[i];
                fieldOrderings[i] = new FieldOrdering
                {
                    Field = Visit(field.Field),
                    Order = field.Order
                };
                changed |= fieldOrderings[i].Field != field.Field;
            }

            return changed
                ? new OrderByExpression {Fields = fieldOrderings}
                : clause;
        }

        public virtual IJqlClause VisitIsEmpty(IsEmptyExpression clause)
        {
            var field = Visit(clause.Field);
            return field == clause.Field
                ? clause
                : new IsEmptyExpression
                {
                    Field = field,
                    Not = clause.Not
                };
        }

        public virtual IJqlClause VisitUnary(UnaryExpression clause)
        {
            var operand = Visit(clause.Operand);
            return operand == clause.Operand
                ? clause
                : new UnaryExpression
                {
                    Operand = operand,
                    Operator = clause.Operator
                };
        }

        public virtual IJqlClause VisitBinary(BinaryExpression clause)
        {
            var left = Visit(clause.Left);
            var right = Visit(clause.Right);
            return left == clause.Left && right == clause.Right
                ? clause
                : new BinaryExpression
                {
                    Left = left,
                    Right = right,
                    Operator = clause.Operator
                };
        }

        public virtual IJqlClause VisitIn(InExpression clause)
        {
            var field = Visit(clause.Field);
            var changed = false;
            var values = new List<IJqlClause>();
            foreach (var value in clause.Values)
            {
                var v = Visit(value);
                values.Add(v);
                changed |= v != value;
            }

            return field == clause.Field && !changed
                ? clause
                : new InExpression
                {
                    Field = field,
                    Not = clause.Not,
                    Values = values
                };
        }

        public virtual IJqlClause VisitIssueFunction(IssueFunctionExpression clause)
        {
            var subQuery = Visit(clause.SubQuery);
            return subQuery == clause.SubQuery
                ? clause
                : new IssueFunctionExpression
                {
                    Function = clause.Function,
                    QuotingType = clause.QuotingType,
                    SubQuery = subQuery
                };
        }

        public virtual IJqlClause VisitCascadeOption(CascadeOptionExpression clause)
        {
            var field = Visit(clause.Field);
            var changed = false;
            var values = new IJqlClause[clause.Values.Length];
            for (var i = 0; i < clause.Values.Length; ++i)
            {
                var value = clause.Values[i];
                var v = Visit(value);
                values[i] = v;
                changed |= v != value;
            }

            return field == clause.Field && !changed
                ? clause
                : new CascadeOptionExpression
                {
                    Field = field,
                    Values = values
                };
        }

        public virtual IJqlClause VisitJql(JqlExpression clause)
        {
            var changed = false;
            IJqlClause filter = null;
            OrderByExpression orderBy = null;
            if (clause.Filter != null)
            {
                filter = Visit(clause.Filter);
                changed |= filter != clause.Filter;
            }

            if (clause.OrderBy != null)
            {
                orderBy = (OrderByExpression) Visit(clause.OrderBy);
                changed |= orderBy != clause.OrderBy;
            }

            return changed
                ? new JqlExpression
                {
                    Filter = filter,
                    OrderBy = orderBy
                }
                : clause;
        }
    }
}