using System;
using System.Collections.Generic;
using System.Linq;
using Irony.Parsing;
using SimpleJira.Impl.Mock.Jql.Expressions;
using SimpleJira.Impl.Utilities;

namespace SimpleJira.Impl.Mock.Jql.Parser
{
    internal class JqlGrammar : Grammar
    {
        private readonly NumberLiteral numberLiteral =
            new NumberLiteral("number", NumberOptions.Default,
                (context, node) => node.AstNode = new LiteralExpression {Value = node.Token.Value});

        public JqlGrammar() : base(false)
        {
            LanguageFlags = LanguageFlags.CreateAst;

            var not = Transient("not", ToTerm("NOT"));

            var expression = NonTerminal("expression", null, TermFlags.IsTransient);
            var identifier = Identifier();

            var doubleQutedStringLiteral = new StringLiteral("doubleQuotedString",
                "\"",
                StringOptions.AllowsAllEscapes | StringOptions.AllowsDoubledQuote,
                (context, node) => node.AstNode = new LiteralExpression
                {
                    Value = node.Token.Value,
                    Quoting = LiteralExpressionQuotingType.Double
                });
            var singleQutedStringLiteral = new StringLiteral("singleQuotedString",
                "'",
                StringOptions.AllowsAllEscapes | StringOptions.AllowsDoubledQuote,
                (context, node) => node.AstNode = new LiteralExpression
                {
                    Value = node.Token.Value,
                    Quoting = LiteralExpressionQuotingType.Single
                });

            var stringLiteral = NonTerminal("stringLiteral", null, n => n.ChildNodes[0].AstNode);
            stringLiteral.Rule = doubleQutedStringLiteral | singleQutedStringLiteral;

            var emptyLiteral = NonTerminal("nullLiteral",
                ToTerm("empty"), node => new LiteralExpression());

            var term = NonTerminal("term", null, TermFlags.IsTransient);

            term.Rule = identifier | stringLiteral | numberLiteral | emptyLiteral;

            var unOp = NonTerminal("unOp", null, ToUnaryOperator);
            unOp.SetFlag(TermFlags.InheritPrecedence);
            var unExpr = NonTerminal("unExpr", null, ToUnaryExpression);
            unOp.Rule = not;
            unExpr.Rule = unOp + expression;


            var binOp = NonTerminal("binOp", null, ToBinaryOperator);
            binOp.SetFlag(TermFlags.InheritPrecedence);
            var binExpr = NonTerminal("binExpr", null, ToBinaryExpression);
            binOp.Rule = ToTerm("=") | "!=" | "~" | "!~" | ">" | "<" | ">=" | "<=" | "AND" | "OR";
            binExpr.Rule = expression + binOp + expression;

            var inExpr = NonTerminal("inExpr", null, x => ToInExpression(x, false));
            var notInExpr = NonTerminal("notInExpr", null, x => ToInExpression(x, true));
            var exprList = NonTerminal("exprList", null);
            exprList.Rule = MakeStarRule(exprList, ToTerm(","), expression);
            var parExprList = NonTerminal("parExprList", null, TermFlags.IsTransient);
            parExprList.Rule = "(" + exprList + ")";
            var functionArgs = NonTerminal("funArgs", null, TermFlags.IsTransient);
            functionArgs.Rule = parExprList;
            inExpr.Rule = identifier
                          + Transient("in", ToTerm("IN"))
                          + functionArgs;
            notInExpr.Rule = identifier
                             + not
                             + Transient("in", ToTerm("IN"))
                             + functionArgs;
            var isNull = NonTerminal("isNull", null, TermFlags.NoAstNode);
            var isNullExpression = NonTerminal("isNullExpression", null, ToIsNullExpression);
            isNull.Rule = NonTerminal("is", ToTerm("IS")) + (Empty | not) + "EMPTY";
            isNullExpression.Rule = term + isNull;
            expression.Rule = term | unExpr | binExpr | isNullExpression | inExpr | notInExpr;

            RegisterOperators<JqlBinaryOperator>();
            RegisterOperators<UnaryOperator>();
            MarkPunctuation(",", "(", ")");
            AddOperatorReportGroup("operator");
            Root = expression;
        }

        private void RegisterOperators<TEnum>() where TEnum : struct
        {
            foreach (var value in Enum.GetValues(typeof(TEnum)).Cast<TEnum>())
            {
                var precedence = EnumAttributesCache<OperatorPrecedenceAttribute>.GetAttribute(value).Precedence;
                var opSymbols = EnumAttributesCache<OperatorSynonymsAttribute>.GetAttribute(value).Synonyms;
                RegisterOperators(precedence, opSymbols);
            }
        }

        private static object ToUnaryExpression(ParseTreeNode node)
        {
            return new UnaryExpression
            {
                Operator = (UnaryOperator) node.ChildNodes[0].AstNode,
                Argument = (IJqlClause) node.ChildNodes[1].AstNode
            };
        }

        private static JqlBinaryOperator ToBinaryOperator(ParseTreeNode node)
        {
            var operatorText = node.FindTokenAndGetText().ToLower();
            switch (operatorText)
            {
                case "and":
                    return JqlBinaryOperator.And;
                case "or":
                    return JqlBinaryOperator.Or;
                case "=":
                    return JqlBinaryOperator.Eq;
                case ">":
                    return JqlBinaryOperator.GreaterThan;
                case "<":
                    return JqlBinaryOperator.LessThan;
                case ">=":
                    return JqlBinaryOperator.GreaterThanOrEqual;
                case "<=":
                    return JqlBinaryOperator.LessThanOrEqual;
                case "~":
                    return JqlBinaryOperator.Contains;
                case "!=":
                    return JqlBinaryOperator.Neq;
                case "!~":
                    return JqlBinaryOperator.NotContains;
                default:
                    throw new InvalidOperationException($"unexpected binary operator [{operatorText}]");
            }
        }

        private static BinaryExpression ToBinaryExpression(ParseTreeNode node)
        {
            var left = (IJqlClause) node.ChildNodes[0].AstNode;
            var binaryOperator = (JqlBinaryOperator) node.ChildNodes[1].AstNode;
            var right = (IJqlClause) node.ChildNodes[2].AstNode;
            return new BinaryExpression(binaryOperator)
            {
                Left = left,
                Right = right
            };
        }

        private static UnaryOperator ToUnaryOperator(ParseTreeNode node)
        {
            var text = node.FindTokenAndGetText().ToLower();
            switch (text)
            {
                case "not":
                    return UnaryOperator.Not;
                default:
                    throw new InvalidOperationException($"unexpected unary operator [{text}]");
            }
        }

        private static InExpression ToInExpression(ParseTreeNode node, bool notIn)
        {
            var sourceNode = node.ChildNodes[notIn ? 3 : 2];
            var sqlSource = sourceNode.AstNode as IJqlClause;
            return new InExpression
            {
                NotIn = notIn,
                Field = (IdentifierExpression) node.ChildNodes[0].AstNode,
                Source = sqlSource ?? new ListExpression
                {
                    Elements = sourceNode.Elements().Cast<IJqlClause>().ToList()
                }
            };
        }

        private static IsEmptyExpression ToIsNullExpression(ParseTreeNode arg)
        {
            var notNode = arg.ChildNodes[1].ChildNodes[1];
            var notToken = notNode.ChildNodes.Any() ? notNode.ChildNodes[0].Token : null;
            return new IsEmptyExpression
            {
                Argument = (IJqlClause) arg.ChildNodes[0].AstNode,
                IsNotEmpty = notToken != null && "not".Equals(notToken.ValueString, StringComparison.OrdinalIgnoreCase)
            };
        }

        private static NonTerminal Identifier()
        {
            var idSimple = new RegexBasedTerminal("[\\w\\d\\-_]+");
            idSimple.SetFlag(TermFlags.NoAstNode);
            var customFieldIdentifier = new RegexBasedTerminal("customFieldExpr", "cf\\[\\d+\\]");
            customFieldIdentifier.SetFlag(TermFlags.NoAstNode);
            var id = NonTerminal("identifier", null, n => new IdentifierExpression
            {
                Value = n.ChildNodes.Single().Token.ValueString
            });
            id.Rule = idSimple | customFieldIdentifier;
            return id;
        }

        private static NonTerminal Transient(string name, BnfExpression rule)
        {
            return new NonTerminal(name, rule) {Flags = TermFlags.IsTransient};
        }

        private static NonTerminal NonTerminal(string name, BnfExpression rule, TermFlags flags = TermFlags.None)
        {
            var nonTerminal = NonTerminal(name, rule, n => new ElementsHolder
            {
                DebugName = name,
                Elements = n.Elements()
            });
            nonTerminal.SetFlag(flags);
            return nonTerminal;
        }

        private static NonTerminal NonTerminal<T>(string name, BnfExpression rule, Func<ParseTreeNode, T> creator)
        {
            return new NonTerminal(name, (context, n) =>
            {
                try
                {
                    n.AstNode = creator(n);
                }
                catch (Exception e)
                {
                    var input = string.Join(" ", GetTokens(n));
                    const string messageFormat = "exception creating ast node from node {0} [{1}]";
                    throw new InvalidOperationException(string.Format(messageFormat, n, input), e);
                }
            })
            {
                Rule = rule
            };
        }

        private static IEnumerable<Token> GetTokens(ParseTreeNode node)
        {
            if (node.Token != null)
                return new[] {node.Token};
            return node.ChildNodes.SelectMany(GetTokens);
        }
    }
}