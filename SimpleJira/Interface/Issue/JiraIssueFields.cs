using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using SimpleJira.Impl.Helpers;
using SimpleJira.Impl.Serialization;

namespace SimpleJira.Interface.Issue
{
    public class JiraIssueFields
    {
        private readonly ConcurrentDictionary<string, object> jObject;

        public JiraIssueFields()
        {
            jObject = new ConcurrentDictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
        }

        public JiraIssueFields(Dictionary<string, object> fields)
        {
            jObject = new ConcurrentDictionary<string, object>(fields, StringComparer.InvariantCultureIgnoreCase);
        }

        public TValue GetProperty<TValue>(string property)
        {
            return (TValue) GetProperty(property, typeof(TValue));
        }

        public object GetProperty(string property, Type type)
        {
            TryGetProperty(property, type, out var result);
            return result;
        }

        public bool TryGetProperty(string property, Type type, out object value)
        {
            if (jObject.TryGetValue(property, out var obj))
            {
                JsonNode jsonNode = JsonSerializer.SerializeToNode(obj);
                value = Json.FromToken(jsonNode, type);
                return true;
            }

            value = type.GetDefaultValue();
            return false;
        }

        public void SetProperty(string property, object value)
        {
            if (jObject.TryGetValue(property, out _))
                jObject[property] = value == null ? null : Json.ToToken(value);
            else
                jObject.TryAdd(property, value == null ? null : Json.ToToken(value));
        }

        public string ToJson()
        {
            return Json.Serialize(jObject);
        }

        public static JiraIssueFields FromJson(string json)
        {
            return new JiraIssueFields(Json.Deserialize<Dictionary<string, object>>(json));
        }

        public JiraIssueFields Clone()
        {
            return FromJson(ToJson());
        }

        public void CopyTo(JiraIssueFields target)
        {
            foreach (var property in jObject)
                target.SetProperty(property.Key, property.Value);
        }

        public void CopyTo(JiraIssueFields target, HashSet<string> except)
        {
            if (except == null)
                throw new ArgumentNullException(nameof(except));
            foreach (var property in jObject)
                if (!except.Contains(property.Key))
                    target.SetProperty(property.Key, property.Value);
        }
    }
}