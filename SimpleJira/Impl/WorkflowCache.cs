using System;
using System.Collections.Concurrent;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl
{
    internal static class WorkflowCache
    {
        public static readonly ConcurrentDictionary<Type, Workflow>
            transitions = new ConcurrentDictionary<Type, Workflow>();

        public static readonly Type[] constructorParameterTypes = new Type[0];
        public static readonly object[] constructorParameters = new object[0];
        public static readonly JiraTransition[] emptyTransitions = new JiraTransition[0];
        public static readonly Func<JiraIssue, bool> defaultCondition = i => true;
    }
}