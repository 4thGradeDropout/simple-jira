using System;
using System.Linq;
using System.Text.RegularExpressions;
using Irony.Parsing;
using SimpleJira.Impl.Ast;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface;

namespace SimpleJira.Fakes.Impl.Jql.Parser
{
    internal class JqlGrammar : Grammar
    {
        private static readonly Regex customFieldRegex =
            new Regex("cf\\[(\\d+)\\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public JqlGrammar()
        {
            LanguageFlags = LanguageFlags.CreateAst;

            var COMMA = ToTerm(",");
            var CONTAINS = ToTerm("~");
            var NOT_CONTAINS = ToTerm("!~");
            var EQ = ToTerm("=");
            var NOT_EQ = ToTerm("!=");
            var GT = ToTerm(">");
            var GTE = ToTerm(">=");
            var LT = ToTerm("<");
            var LTE = ToTerm("<=");
            var NOT = ToTerm("NOT");
            var OR = ToTerm("OR");
            var AND = ToTerm("AND");
            var IN = ToTerm("IN");
            var IS = ToTerm("IS");
            var EMPTY = ToTerm("EMPTY");
            var BY = ToTerm("BY");
            var ASC = ToTerm("ASC");
            var DESC = ToTerm("DESC");
            var ISSUEFUNCTION = ToTerm("ISSUEFUNCTION");
            var PARENTS_OF = ToTerm("PARENTSOF");
            var SUBTASKS_OF = ToTerm("SUBTASKSOF");
            var CASCADEOPTION = ToTerm("CASCADEOPTION");
            var ORDER = ToTerm("ORDER");

            var simpleStringLiteral =
                new RegexBasedTerminal("simpleStringLiteral",
                    "(?!NOT\\b)(?!OR\\b)(?!AND\\b)(?!IS\\b)(?!EMPTY\\b)(?!ISSUEFUNCTION\\b)(?!CASCADEOPTION\\b)(?!ORDER\\b)(?!BY\\b)(?!ASC\\b)(?!DESC\\b)[\\w\\d\\*\\.\\-_]+");
            var simpleStringTerm = new NonTerminal("simpleStringTerm", (c, n) => BuildSimpleLiteral(n));
            var doubleQuotedStringLiteral = new StringLiteral("doubleQuotedStringLiteral", "\"",
                StringOptions.AllowsAllEscapes, (c, n) => BuildQuotedLiteral(n, JqlLiteralQuotingType.Double));
            var singleQuotedStringLiteral = new StringLiteral("singleQuotedStringLiteral", "'",
                StringOptions.AllowsAllEscapes, (c, n) => BuildQuotedLiteral(n, JqlLiteralQuotingType.Single));
            var stringExpr = new NonTerminal("stringExpr", (c, n) => BuildSingleChild(n));
            var customFieldLiteral = new RegexBasedTerminal("customFieldLiteral", "CF\\[\\d+\\]");
            var customFieldTerm = new NonTerminal("customFieldTerm", (c, n) => BuildCustomField(n));
            var fieldExpr = new NonTerminal("fieldExpr", (c, n) => BuildFieldReference(n));
            var expression = new NonTerminal("expression", (c, n) => BuildSingleChild(n));
            var fieldMatchingOp = new NonTerminal("fieldMatchingOp", (c, n) => BuildFieldMatchingOp(n));
            var fieldMatchingExpr = new NonTerminal("fieldMatchingExpr", (c, n) => BuildFieldMatching(n));
            var isEmptyExpr = new NonTerminal("isEmptyExpr", (c, n) => BuildIsEmpty(n));
            var isNotEmptyExpr = new NonTerminal("isNotEmptyExpr", (c, n) => BuildIsEmpty(n));
            var unOp = new NonTerminal("unOp", (c, n) => n.AstNode = JqlUnaryExpressionType.Not);
            var unExpr = new NonTerminal("unExpr", (c, n) => BuildUnary(n));
            var binOp = new NonTerminal("binOp", (c, n) => BuildBinaryOperator(n));
            var binExpr = new NonTerminal("binExpr", (c, n) => BuildBinary(n));
            var expressionList = new NonTerminal("expressionList", (c, n) => BuildElementsHolder(n));
            var inExpr = new NonTerminal("inExpr", (c, n) => BuildIn(n));
            var notInExpr = new NonTerminal("notInExpr", (c, n) => BuildIn(n));
            var issueFunctionNameExpr = new NonTerminal("issueFunctionNameExpr", (c, n) => BuildIssueFunctionName(n));
            var issueFunctionExpr = new NonTerminal("issueFunctionExpr", (c, n) => BuildIssueFunction(n));
            var cascadeOptionExpr = new NonTerminal("cascadeOptionExpr", (c, n) => BuildCascadeOption(n));
            var orderByElementExpr = new NonTerminal("orderByElementExpr", (c, n) => BuildOrderByField(n));
            var orderByElementListExpr = new NonTerminal("orderByElementListExpr", (c, n) => BuildElementsHolder(n));
            var orderByExpr = new NonTerminal("orderByExpr", (c, n) => BuildOrderBy(n));
            var jqlQuery = new NonTerminal("jqlQuery", (context, node) => BuildJql(node));

            simpleStringLiteral.SetFlag(TermFlags.NoAstNode | TermFlags.IsLiteral);
            customFieldLiteral.SetFlag(TermFlags.NoAstNode);
            customFieldTerm.Rule = customFieldLiteral;
            simpleStringTerm.Rule = simpleStringLiteral;
            stringExpr.Rule = simpleStringTerm | singleQuotedStringLiteral | doubleQuotedStringLiteral;
            fieldExpr.Rule = customFieldTerm | stringExpr;
            fieldMatchingOp.Rule = EQ | NOT_EQ | CONTAINS | NOT_CONTAINS | GT | GTE | LT | LTE;
            fieldMatchingOp.SetFlag(TermFlags.InheritPrecedence);
            unOp.SetFlag(TermFlags.InheritPrecedence);
            unOp.Rule = NOT;
            binOp.SetFlag(TermFlags.InheritPrecedence);
            binOp.Rule = OR | AND;
            fieldMatchingExpr.Rule = fieldExpr + fieldMatchingOp + stringExpr;
            isEmptyExpr.Rule = fieldExpr + IS + EMPTY;
            isNotEmptyExpr.Rule = fieldExpr + IS + NOT + EMPTY;
            unExpr.Rule = unOp + expression;
            binExpr.Rule = expression + binOp + expression;
            MakePlusRule(expressionList, COMMA, stringExpr);
            IN.SetFlag(TermFlags.InheritPrecedence);
            inExpr.Rule = fieldExpr + IN + "(" + expressionList + ")";
            notInExpr.Rule = fieldExpr + NOT + IN + "(" + expressionList + ")";
            issueFunctionNameExpr.Rule = PARENTS_OF | SUBTASKS_OF;
            issueFunctionExpr.Rule = ISSUEFUNCTION + IN + issueFunctionNameExpr + "(" + stringExpr + ")";
            cascadeOptionExpr.Rule = fieldExpr + IN + CASCADEOPTION + "(" + expressionList + ")";

            expression.Rule = (ToTerm("(") + expression + ")") | unExpr | binExpr | inExpr | notInExpr |
                              fieldMatchingExpr | isEmptyExpr | isNotEmptyExpr | issueFunctionExpr |
                              cascadeOptionExpr;

            orderByElementExpr.Rule = fieldExpr | (fieldExpr + ASC) | (fieldExpr + DESC);
            MakePlusRule(orderByElementListExpr, COMMA, orderByElementExpr);

            orderByExpr.Rule = ORDER + BY + orderByElementListExpr;
            jqlQuery.Rule = expression | orderByExpr | expression + orderByExpr;

            RegisterBracePair("(", ")");
            MarkPunctuation("(", ")");
            RegisterOperators(7, OR, AND);
            RegisterOperators(9, NOT);
            RegisterOperators(10, CONTAINS, EQ, NOT_EQ, GT, GTE, LT, LTE, IN, IS);

            Root = jqlQuery;
        }

        private void BuildOrderBy(ParseTreeNode n)
        {
            n.AstNode = new OrderByExpression
            {
                Fields = ((ElementsHolder) n.ChildNodes[2].AstNode).Elements
                    .Cast<FieldOrdering>()
                    .ToArray()
            };
        }

        private static void BuildOrderByField(ParseTreeNode n)
        {
            var field = (IJqlClause) n.ChildNodes[0].AstNode;
            if (n.ChildNodes.Count == 1)
            {
                n.AstNode = new FieldOrdering
                {
                    Field = field,
                    Order = JqlOrderType.Asc
                };
            }
            else
            {
                var order = n.ChildNodes[1].Token.ValueString;
                n.AstNode = new FieldOrdering
                {
                    Field = field,
                    Order = order == "DESC" ? JqlOrderType.Desc : JqlOrderType.Asc
                };
            }
        }

        private static void BuildIssueFunctionName(ParseTreeNode n)
        {
            var function = n.ChildNodes[0].Token.ValueString;
            n.AstNode = JqlIssueFunctionHelpers.Parse(function);
        }

        private static void BuildIssueFunction(ParseTreeNode n)
        {
            var function = (JqlIssueFunction) n.ChildNodes[2].AstNode;
            var stringExpression = (ValueExpression) n.ChildNodes[3].AstNode;
            var subQuery = JqlParser.Parse(stringExpression.Value);
            if (subQuery == null)
                throw new JqlCompilationException($"subquery of issueFunction [{function}] should not be empty");
            n.AstNode = new IssueFunctionExpression
            {
                QuotingType = stringExpression.QuotingType.GetValueOrDie(),
                Function = function,
                SubQuery = subQuery
            };
        }

        private static void BuildCascadeOption(ParseTreeNode n)
        {
            var field = (IJqlClause) n.ChildNodes[0].AstNode;
            var values = ((ElementsHolder) n.ChildNodes[3].AstNode).Elements.Cast<IJqlClause>().ToArray();
            n.AstNode = new CascadeOptionExpression
            {
                Field = field,
                Values = values
            };
        }

        private static void BuildIsEmpty(ParseTreeNode n)
        {
            n.AstNode = new IsEmptyExpression
            {
                Field = (IJqlClause) n.ChildNodes[0].AstNode,
                Not = n.ChildNodes.Count == 4
            };
        }

        private static void BuildFieldMatchingOp(ParseTreeNode n)
        {
            var op = n.ChildNodes[0].Token.ValueString;
            n.AstNode = op switch
            {
                "~" => JqlFieldMatchingType.Contains,
                "!~" => JqlFieldMatchingType.NotContains,
                "=" => JqlFieldMatchingType.Equals,
                "!=" => JqlFieldMatchingType.NotEquals,
                "<" => JqlFieldMatchingType.Less,
                "<=" => JqlFieldMatchingType.LessOrEquals,
                ">" => JqlFieldMatchingType.Greater,
                ">=" => JqlFieldMatchingType.GreaterOrEquals,
                _ => throw new JqlParseError($"unknown field matching operator [{op}]")
            };
        }

        private static void BuildCustomField(ParseTreeNode n)
        {
            var field = n.ChildNodes.Single().Token.ValueString;
            n.AstNode = new CustomField
            {
                Number = int.Parse(customFieldRegex.Match(field).Groups[1].Value)
            };
        }

        private static void BuildIn(ParseTreeNode n)
        {
            var not = n.ChildNodes.Count == 4;
            n.AstNode = new InExpression
            {
                Field = (IJqlClause) n.ChildNodes[0].AstNode,
                Values = ((ElementsHolder) n.ChildNodes[not ? 3 : 2].AstNode).Elements.Cast<IJqlClause>().ToList(),
                Not = not
            };
        }

        private static void BuildElementsHolder(ParseTreeNode n)
        {
            n.AstNode = new ElementsHolder
            {
                DebugName = "expressionList",
                Elements = n.Elements()
            };
        }

        private static void BuildFieldMatching(ParseTreeNode n)
        {
            n.AstNode = new FieldMatchingExpression
            {
                Field = (IJqlClause) n.ChildNodes[0].AstNode,
                Operator = (JqlFieldMatchingType) n.ChildNodes[1].AstNode,
                Value = (IJqlClause) n.ChildNodes[2].AstNode
            };
        }

        private static void BuildBinary(ParseTreeNode n)
        {
            n.AstNode = new BinaryExpression
            {
                Left = (IJqlClause) n.ChildNodes[0].AstNode,
                Operator = (JqlBinaryExpressionType) n.ChildNodes[1].AstNode,
                Right = (IJqlClause) n.ChildNodes[2].AstNode
            };
        }

        private static void BuildUnary(ParseTreeNode n)
        {
            n.AstNode = new UnaryExpression
            {
                Operator = (JqlUnaryExpressionType) n.ChildNodes[0].AstNode,
                Operand = (IJqlClause) n.ChildNodes[1].AstNode
            };
        }

        private static void BuildSingleChild(ParseTreeNode n)
        {
            n.AstNode = n.ChildNodes.Single().AstNode;
        }

        private static void BuildSimpleLiteral(ParseTreeNode n)
        {
            n.AstNode = new ValueExpression
            {
                Value = n.ChildNodes[0].Token.ValueString,
                QuotingType = null
            };
        }

        private static void BuildQuotedLiteral(ParseTreeNode n, JqlLiteralQuotingType type)
        {
            n.AstNode = new ValueExpression
            {
                Value = n.Token.ValueString,
                QuotingType = type
            };
        }

        private static void BuildBinaryOperator(ParseTreeNode n)
        {
            var op = n.ChildNodes[0].Token.ValueString;
            if ("OR".Equals(op, StringComparison.InvariantCultureIgnoreCase))
                n.AstNode = JqlBinaryExpressionType.Or;
            else if ("AND".Equals(op, StringComparison.InvariantCultureIgnoreCase))
                n.AstNode = JqlBinaryExpressionType.And;
            else
                throw new JqlParseError($"unknown binary operator [{op}]");
        }

        private static void BuildFieldReference(ParseTreeNode n)
        {
            var node = n.ChildNodes.Single().AstNode;
            if (node is CustomField customField)
                n.AstNode = new FieldReferenceExpression
                {
                    Field = "CUSTOMFIELD_" + customField.Number,
                    CustomId = customField.Number
                };
            else if (node is ValueExpression valueExpression)
                n.AstNode = new FieldReferenceExpression
                {
                    Field = valueExpression.Value,
                };
            else
                throw new JqlParseError($"can not parse field reference expression [{n}]");
        }

        private static void BuildJql(ParseTreeNode n)
        {
            if (n.ChildNodes.Count == 1)
            {
                var node = (IJqlClause) n.ChildNodes[0].AstNode;
                if (node is OrderByExpression orderBy)
                {
                    n.AstNode = new JqlExpression
                    {
                        OrderBy = orderBy
                    };
                }
                else
                {
                    n.AstNode = new JqlExpression
                    {
                        Filter = node
                    };
                }
            }
            else
            {
                n.AstNode = new JqlExpression
                {
                    Filter = (IJqlClause) n.ChildNodes[0].AstNode,
                    OrderBy = (OrderByExpression) n.ChildNodes[1].AstNode
                };
            }
        }
    }
}