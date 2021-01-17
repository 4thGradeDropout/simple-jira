using System;
using System.Collections.Generic;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl
{
    internal class Workflow
    {
        private readonly Dictionary<string, JiraTransition[]> transitions;
        private readonly Dictionary<string, Func<JiraIssue, bool>> conditions;

        public Workflow(JiraIssueType issueType, JiraStatus defaultStatus,
            Dictionary<string, JiraTransition[]> transitions,
            Dictionary<string, Func<JiraIssue, bool>> conditions)
        {
            this.transitions = transitions;
            this.conditions = conditions;
            IssueType = issueType;
            DefaultStatus = defaultStatus;
        }

        public JiraIssueType IssueType { get; }
        public JiraStatus DefaultStatus { get; }

        public Func<JiraIssue, bool> GetCondition(JiraTransition transition)
        {
            return conditions.TryGetValue(transition.Id, out var condition)
                ? condition
                : WorkflowCache.defaultCondition;
        }

        public JiraTransition[] GetTransitions(JiraStatus status)
        {
            return transitions.TryGetValue(status.Id, out var items)
                ? items
                : WorkflowCache.emptyTransitions;
        }
    }
}