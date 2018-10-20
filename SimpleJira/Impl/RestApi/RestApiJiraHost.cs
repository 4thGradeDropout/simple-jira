using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using SimpleJira.Impl.Utilities;
using SimpleJira.Interface;
using SimpleJira.Interface.ObjectModel;
using SimpleJira.Interface.RestApi;

namespace SimpleJira.Impl.RestApi
{
    internal class RestApiJiraHost : IJiraHost
    {
        private const string jsonMimeType = "application/json";
        private readonly JiraEndPoint endPoint;

        public RestApiJiraHost(JiraEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        public JiraQueryResponse Query(JiraQuery query)
        {
            var parameters = CreateParameters(query);
            var url = "/rest/api/2/search";
            if (parameters.Count > 0)
                url += "?" + ToQueryString(parameters);
            var body = Get(url);
            var json = Encoding.UTF8.GetString(body);
            var jiraResponse = JsonConvert.DeserializeObject<JiraQueryResponseModel>(json);
            var issues = new JiraIssue[jiraResponse.Issues.Length];
            for (var i = 0; i < issues.Length; ++i)
            {
                var current = jiraResponse.Issues[i];
                issues[i] = new JiraIssue
                {
                    Key = current.Key,
                    Id = current.Id,
                    Fields = current.Fields.ToDictionary()
                };
            }
            return new JiraQueryResponse
            {
                Expand = jiraResponse.Expand,
                Issues = issues,
                MaxResults = jiraResponse.MaxResults,
                StartAt = jiraResponse.StartAt,
                Total = jiraResponse.Total
            };
        }

        public string CreateIssue(object fields)
        {
            var json = JsonConvert.SerializeObject(new {fields});
            var body = Encoding.UTF8.GetBytes(json);
            var result = Post("/rest/api/2/issue", body);
            var resultJson = Encoding.UTF8.GetString(result);
            return JsonConvert.DeserializeObject<CreateIssueResponse>(resultJson).Key;
        }

        public void UpdateIssue(string issueKey, object fields)
        {
            var json = JsonConvert.SerializeObject(new {fields});
            Put("/rest/api/2/issue/" + issueKey, Encoding.UTF8.GetBytes(json));
        }

        public JiraComment[] GetComments(string issueKey)
        {
            var body = Get("/rest/api/2/issue/" + issueKey + "/comment?maxResults=5000");
            var json = Encoding.UTF8.GetString(body);
            return JsonConvert.DeserializeObject<JiraApiCommentsResponseModel>(json).Comments.Select(x =>
                new JiraComment
                {
                    Author = x.Author == null ? null : FromAuthorModel(x.Author),
                    Body = x.Body,
                    Created = x.Created,
                    UpdateAuthor = x.UpdateAuthor == null ? null : FromAuthorModel(x.UpdateAuthor),
                    Updated = x.Updated
                }).ToArray();
        }

        public void AddComment(string issueKey, JiraComment comment)
        {
            var body = JsonConvert.SerializeObject(new
            {
                body = comment.Body
            });
            Post("/rest/api/2/issue/" + issueKey + "/comment", Encoding.UTF8.GetBytes(body));
        }

        private byte[] Get(string url)
        {
            return Send(url, client => client.GetAsync(url).Result);
        }

        private byte[] Post(string url, byte[] body)
        {
            return Send(url, client =>
            {
                var content = new StringContent(Encoding.UTF8.GetString(body), Encoding.UTF8, jsonMimeType);
                return client.PostAsync(url, content).Result;
            });
        }

        private void Put(string url, byte[] body)
        {
            Send(url, client =>
            {
                var content = new StringContent(Encoding.UTF8.GetString(body), Encoding.UTF8, jsonMimeType);
                return client.PutAsync(url, content).Result;
            });
        }

        private byte[] Send(string url, Func<HttpClient, HttpResponseMessage> sender)
        {
            using (var client = CreateHttpClient(url))
            {
                var response = sender(client);
                var body = response.Content.ReadAsByteArrayAsync().Result;
                if (!response.IsSuccessStatusCode)
                    throw new JiraHttpTransportException(endPoint, response.StatusCode, body);
                return body;
            }
        }

        private HttpClient CreateHttpClient(string url)
        {
            var client = new HttpClient
            {
                BaseAddress = new Uri(endPoint.Url + url)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(jsonMimeType));
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                    Convert.ToBase64String(Encoding.UTF8.GetBytes(endPoint.User + ":" + endPoint.Password)));
            return client;
        }

        private static NameValueCollection CreateParameters(JiraQuery query)
        {
            var parameters = new NameValueCollection();
            if (!string.IsNullOrWhiteSpace(query.Expand))
                parameters.Add("expand", query.Expand);
            if (query.MaxResults.HasValue)
                parameters.Add("maxResults", query.MaxResults.ToString());
            if (query.StartAt.HasValue)
                parameters.Add("startAt", query.StartAt.ToString());
            if (query.Fields != null && query.Fields.Any())
                parameters.Add("fields", string.Join(",", query.Fields));
            if (!string.IsNullOrWhiteSpace(query.Jql))
                parameters.Add("jql", query.Jql);
            return parameters;
        }

        private static JiraCommentAuthor FromAuthorModel(JiraCommentAuthorModel author)
        {
            return new JiraCommentAuthor
            {
                Active = author.Active,
                DisplayName = author.DisplayName,
                Name = author.Name,
                Self = author.Self
            };
        }

        private static string ToQueryString(NameValueCollection collection)
        {
            return collection == null
                ? ""
                : string.Join("&", collection
                    .Cast<string>()
                    .Select(x => $"{Uri.EscapeDataString(x)}={Uri.EscapeDataString(collection[x])}"));
        }

        private class JiraQueryResponseModel
        {
            [JsonProperty("expand")]
            public string Expand { get; set; }

            [JsonProperty("issues")]
            public JiraIssueModel[] Issues { get; set; }

            [JsonProperty("maxResults")]
            public int MaxResults { get; set; }

            [JsonProperty("startAt")]
            public int StartAt { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }
        }


        private class JiraIssueModel
        {
            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("fields")]
            public Dictionary<string, object> Fields { get; set; }
        }

        private class CreateIssueResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("key")]
            public string Key { get; set; }

            [JsonProperty("self")]
            public string Self { get; set; }
        }

        private class JiraApiCommentsResponseModel
        {
            [JsonProperty("comments")]
            public JiraCommentModel[] Comments { get; set; }

            [JsonProperty("maxResults")]
            public int MaxResults { get; set; }

            [JsonProperty("startAt")]
            public int StartAt { get; set; }

            [JsonProperty("total")]
            public int Total { get; set; }
        }

        private class JiraCommentModel
        {
            [JsonProperty("author")]
            public JiraCommentAuthorModel Author { get; set; }

            [JsonProperty("updateAuthor")]
            public JiraCommentAuthorModel UpdateAuthor { get; set; }

            [JsonProperty("body")]
            public string Body { get; set; }

            [JsonProperty("created")]
            public DateTime Created { get; set; }

            [JsonProperty("updated")]
            public DateTime Updated { get; set; }
        }

        private class JiraCommentAuthorModel
        {
            [JsonProperty("self")]
            public string Self { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("displayName")]
            public string DisplayName { get; set; }

            [JsonProperty("active")]
            public bool Active { get; set; }
        }
    }
}