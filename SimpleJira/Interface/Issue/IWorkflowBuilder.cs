using System;
using System.Collections.Generic;
using SimpleJira.Interface.Types;

namespace SimpleJira.Interface.Issue
{
    public interface IWorkflowBuilder<out TIssue> where TIssue : JiraIssue
    {
        IWorkflowBuilder<TIssue> Type(JiraIssueType type);
        IWorkflowBuilder<TIssue> DefaultStatus(JiraStatus status);
        IWorkflowBuilder<TIssue> Status(JiraStatus status, IEnumerable<JiraTransition> transitions);
        IWorkflowBuilder<TIssue> Condition(JiraTransition transition, Func<TIssue, bool> condition);
    }
}