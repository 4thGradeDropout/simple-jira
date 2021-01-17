using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SimpleJira.Fakes.Interface;
using SimpleJira.Interface;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Logging;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Tests.Integration.Queryable
{
    public abstract class QueryableTestBase : TestBase
    {
        protected IJira jira;
        protected JiraQueryProvider provider;

        private protected abstract IEnumerable<Type> GetIssueTypes();

        protected override void SetUp()
        {
            base.SetUp();
            var metadataProvider = new JiraMetadataProvider(GetIssueTypes());
            jira = FakeJira.InMemory("http://fake.jira.int", new JiraUser(), metadataProvider);
            provider = new JiraQueryProvider(jira, metadataProvider, new LoggingSettings
            {
                Level = LogLevel.None,
                Logger = new ConsoleLogger()
            });
        }

        protected IQueryable<TIssue> Source<TIssue>() where TIssue : JiraIssue
        {
            return provider.GetIssues<TIssue>();
        }

        protected void AssertSingle<TIssue>(IQueryable<TIssue> query, JiraIssueReference reference)
            where TIssue : JiraIssue
        {
            var issues = query.ToArray();
            Assert.That(issues.Length, Is.EqualTo(1));
            Assert.That(issues[0].Key, Is.EqualTo(reference.Key));
        }

        protected void AssertEmpty<TIssue>(IQueryable<TIssue> query) where TIssue : JiraIssue
        {
            var issues = query.ToArray();
            Assert.That(issues.Length, Is.EqualTo(0));
        }
    }
}