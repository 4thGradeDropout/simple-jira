namespace SimpleJira.Impl.Mock.Jql.Expressions
{
    internal enum JqlBinaryOperator
    {
        [OperatorSynonyms("AND")] [OperatorPrecedence(5)] And,
        [OperatorSynonyms("OR")] [OperatorPrecedence(4)] Or,
        [OperatorSynonyms("=")] [OperatorPrecedence(8)] Eq,
        [OperatorSynonyms(">")] [OperatorPrecedence(8)] GreaterThan,
        [OperatorSynonyms("<")] [OperatorPrecedence(8)] LessThan,
        [OperatorSynonyms(">=")] [OperatorPrecedence(8)] GreaterThanOrEqual,
        [OperatorSynonyms("<=")] [OperatorPrecedence(8)] LessThanOrEqual,
        [OperatorSynonyms("~")] [OperatorPrecedence(8)] Contains,
        [OperatorSynonyms("!~")] [OperatorPrecedence(8)] NotContains,
        [OperatorSynonyms("!=")] [OperatorPrecedence(8)] Neq,
    }
}