using System;
using SimpleJira.Impl;
using SimpleJira.Impl.Controllers;
using SimpleJira.Impl.Helpers;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Interface.Issue
{
    public class JiraIssue
    {
        public JiraIssue() : this(new JiraIssueFieldsController(new JiraIssueFields()))
        {
            JiraIssueInitializer.Initialize(this);
        }

        public JiraIssue(IJiraIssueFieldsController controller)
        {
            Controller = controller;
            CustomFields = new JiraIssueCustomFields(controller);
        }

        public IJiraIssueFieldsController Controller { get; }

        public string Expand { get; set; }
        public string Id { get; set; }
        public string Key { get; set; }
        public string Self { get; set; }

        [JiraIssueProperty("assignee")]
        public JiraUser Assignee
        {
            get => Controller.GetValue<JiraUser>("assignee");
            set => Controller.SetValue("assignee", value);
        }

        [JiraIssueProperty("created")]
        public DateTime Created
        {
            get => Controller.GetValue<DateTime>("created");
            set => Controller.SetValue("created", value);
        }

        [JiraIssueProperty("creator")]
        public JiraUser Creator
        {
            get => Controller.GetValue<JiraUser>("creator");
            set => Controller.SetValue("creator", value);
        }

        [JiraIssueProperty("description")]
        public string Description
        {
            get => Controller.GetValue<string>("description");
            set => Controller.SetValue("description", value);
        }

        [JiraIssueProperty("duedate")]
        public DateTime? DueDate
        {
            get => Controller.GetValue<DateTime?>("duedate");
            set => Controller.SetValue("duedate", value);
        }

        [JiraIssueProperty("labels")]
        public string[] Labels
        {
            get => Controller.GetValue<string[]>("labels");
            set => Controller.SetValue("labels", value);
        }

        [JiraIssueProperty("lastViewed")]
        public DateTime? LastViewed
        {
            get => Controller.GetValue<DateTime?>("lastViewed");
            set => Controller.SetValue("lastViewed", value);
        }

        [JiraIssueProperty("priority")]
        public JiraPriority Priority
        {
            get => Controller.GetValue<JiraPriority>("priority");
            set => Controller.SetValue("priority", value);
        }

        [JiraIssueProperty("project")]
        public JiraProject Project
        {
            get => Controller.GetValue<JiraProject>("project");
            set => Controller.SetValue("project", value);
        }

        [JiraIssueProperty("reporter")]
        public JiraUser Reporter
        {
            get => Controller.GetValue<JiraUser>("reporter");
            set => Controller.SetValue("reporter", value);
        }

        [JiraIssueProperty("resolutiondate")]
        public DateTime? ResolutionDate
        {
            get => Controller.GetValue<DateTime?>("resolutiondate");
            set => Controller.SetValue("resolutiondate", value);
        }

        [JiraIssueProperty("updated")]
        public DateTime Updated
        {
            get => Controller.GetValue<DateTime>("updated");
            set => Controller.SetValue("updated", value);
        }

        [JiraIssueProperty("issuetype")]
        public JiraIssueType IssueType
        {
            get => Controller.GetValue<JiraIssueType>("issuetype");
            set => Controller.SetValue("issuetype", value);
        }

        [JiraIssueProperty("status")]
        public JiraStatus Status
        {
            get => Controller.GetValue<JiraStatus>("status");
            set => Controller.SetValue("status", value);
        }

        [JiraIssueProperty("summary")]
        public string Summary
        {
            get => Controller.GetValue<string>("summary");
            set => Controller.SetValue("summary", value);
        }

        [JiraIssueProperty("parent")]
        public JiraIssueReference Parent
        {
            get => Controller.GetValue<JiraIssueReference>("parent");
            set => Controller.SetValue("parent", value);
        }

        [JiraIssueProperty("attachment")]
        public JiraAttachment[] Attachment => Controller.GetValue<JiraAttachment[]>("attachment");

        [JiraIssueProperty("comment")]
        public JiraIssueComments Comment => Controller.GetValue<JiraIssueComments>("comment");

        public JiraIssueCustomFields CustomFields { get; }

        public JiraIssueReference Reference()
        {
            return new JiraIssueReference
            {
                Id = Id,
                Key = Key,
                Self = Self,
            };
        }

        public TIssue Cast<TIssue>() where TIssue : JiraIssue
        {
            return (TIssue) Cast((typeof(TIssue)));
        }

        public JiraIssue Cast(Type issueType)
        {
            return JiraIssueCaster.Cast(this, issueType);
        }
    }
}