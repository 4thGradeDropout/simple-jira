using System;

namespace SimpleJira.Impl.Ast
{
    internal static class JqlIssueFunctionHelpers
    {
        public static JqlIssueFunction Parse(string name)
        {
            if("parentsOf".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return JqlIssueFunction.ParentsOf;
            if("subtasksOf".Equals(name, StringComparison.InvariantCultureIgnoreCase))
                return JqlIssueFunction.SubTasksOf;
            throw new ArgumentOutOfRangeException(nameof(name), name, "unknown issue function name");
        }

        public static string Format(JqlIssueFunction function)
        {
            switch(function)
            {
                case JqlIssueFunction.ParentsOf:
                    return "PARENTSOF";
                case JqlIssueFunction.SubTasksOf:
                    return "SUBTASKSOF";
                default:
                    throw new ArgumentOutOfRangeException(nameof(function), function, null);
            }
        }
    }
}