using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SimpleJira.Impl.Mock.Jql;
using SimpleJira.Impl.Utilities;
using SimpleJira.Interface;
using SimpleJira.Interface.ObjectModel;

namespace SimpleJira.Impl.Mock
{
    internal class MockJiraHost : IJiraHost
    {
        private readonly JiraMetadata metadata;
        private readonly IJiraIssueMockStore store;
        private long id = new Random().Next();

        private readonly object lockObject = new object();

        public MockJiraHost(JiraMetadata metadata, IJiraIssueMockStore store)
        {
            this.metadata = metadata;
            this.store = store;
        }

        public JiraQueryResponse Query(JiraQuery query)
        {
            lock (lockObject)
            {
                var filter = JqlFilterBuilder.Build(metadata, query.Jql);
                return new JiraQueryResponse
                {
                    Issues = store.Select().Where(filter).ToArray()
                };
            }
        }

        public string CreateIssue(object fields)
        {
            lock (lockObject)
            {
                var cloned = ObjectToDictionary(fields).ToDictionary();
                var key = store.KeyPrefix + "-" + ++id;
                store.Create(new JiraIssue
                {
                    Key = key,
                    Id = "Id" + id,
                    Fields = cloned
                });
                return key;
            }
        }

        public void UpdateIssue(string issueKey, object fields)
        {
            lock (lockObject)
            {
                var issue = GetByKey(issueKey);
                UpadteIssueInternal(issue, fields);
            }
        }

        public JiraComment[] GetComments(string key)
        {
            lock (lockObject)
            {
                var issue = GetByKey(key); // Check issue existance
                return store.SelectComments(issue.Key).ToArray();
            }
        }

        public void AddComment(string key, JiraComment comment)
        {
            lock (lockObject)
            {
                var issue = GetByKey(key); // Check issue existance
                store.AddComment(issue.Key, comment);
            }
        }

        private JiraIssue GetByKey(string key)
        {
            var issuesByKey = Query(new JiraQuery {Jql = "KEY = " + key}).Issues
                .Take(2)
                .ToArray();
            if (issuesByKey.Length == 0)
                throw new InvalidOperationException($"issue with key [{key}] was not found");
            if (issuesByKey.Length > 1)
                throw new InvalidOperationException($"more then one issue with key [{key}]");
            var jiraApiIssue = issuesByKey[0];
            return jiraApiIssue;
        }

        private void UpadteIssueInternal(JiraIssue issue, object data)
        {
            var result = issue.Fields.ToDictionary();
            var newProperties = ObjectToDictionary(data);
            foreach (var pair in newProperties)
            {
                if (result.ContainsKey(pair.Key))
                {
                    if (pair.Value == null)
                        result.Remove(pair.Key);
                    else
                        result[pair.Key] = pair.Value;
                }
                else
                    result.Add(pair.Key, pair.Value);
            }
            issue.Fields = result;
            store.Update(issue);
        }

        private static Dictionary<string, object> ObjectToDictionary(object data)
        {
            var json = JsonConvert.SerializeObject(data);
            return JsonConvert.DeserializeObject<Dictionary<string, object>>(json).ToDictionary();
        }
    }
}