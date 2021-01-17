using System;
using System.Collections.Generic;
using System.Linq;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl
{
    internal class WorkflowBuilder<TIssue> : IWorkflowBuilder<TIssue> where TIssue : JiraIssue
    {
        private readonly Dictionary<string, JiraTransition[]> transitions =
            new Dictionary<string, JiraTransition[]>();

        private readonly Dictionary<string, Func<JiraIssue, bool>> conditions =
            new Dictionary<string, Func<JiraIssue, bool>>();

        private JiraIssueType issueType;
        private JiraStatus defaultStatus;

        public IWorkflowBuilder<TIssue> Type(JiraIssueType type)
        {
            issueType = type;
            return this;
        }

        public IWorkflowBuilder<TIssue> DefaultStatus(JiraStatus status)
        {
            defaultStatus = status;
            return this;
        }

        public IWorkflowBuilder<TIssue> Status(JiraStatus status, IEnumerable<JiraTransition> transitions)
        {
            if (this.transitions.ContainsKey(status.Id))
                throw new InvalidOperationException(
                    $"transitions' set is already defined, status name: [{status.Name}], id: [{status.Id}]");
            this.transitions.Add(status.Id, transitions.ToArray());
            return this;
        }

        public IWorkflowBuilder<TIssue> Condition(JiraTransition transition, Func<TIssue, bool> condition)
        {
            if (conditions.ContainsKey(transition.Id))
                throw new InvalidOperationException(
                    $"condition is already define, transition [{transition.Name}], id: [{transition.Id}]");
            conditions.Add(transition.Id, i => condition((TIssue) i));
            return this;
        }

        public Workflow Build()
        {
            return new Workflow(issueType, defaultStatus, transitions, conditions);
        }
    }
}