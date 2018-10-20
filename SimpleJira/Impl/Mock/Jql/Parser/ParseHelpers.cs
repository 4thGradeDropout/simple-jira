using System.Collections.Generic;
using Irony.Parsing;

namespace SimpleJira.Impl.Mock.Jql.Parser
{
    internal static class ParseHelpers
    {
        public static List<object> Elements(this ParseTreeNode n)
        {
            var result = new List<object>();
            foreach (var node in n.ChildNodes)
            {
                if (node.AstNode is ElementsHolder holder)
                {
                    result.AddRange(holder.Elements);
                    continue;
                }
                if (node.AstNode != null)
                    result.Add(node.AstNode);
            }
            return result;
        }
    }
}