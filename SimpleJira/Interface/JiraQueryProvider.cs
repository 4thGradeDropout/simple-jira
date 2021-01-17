using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using SimpleJira.Impl.Helpers;
using SimpleJira.Impl.Queryable;
using SimpleJira.Interface.Issue;
using SimpleJira.Interface.Logging;
using SimpleJira.Interface.Metadata;

namespace SimpleJira.Interface
{
    public class JiraQueryProvider
    {
        private readonly IJira jira;
        private readonly IJiraMetadataProvider metadataProvider;
        private readonly LoggingSettings? loggingSettings;
        private const int packetSize = 100;

        private readonly ConcurrentDictionary<string, byte> warnings = new ConcurrentDictionary<string, byte>();

        public JiraQueryProvider(IJira jira, IJiraMetadataProvider metadataProvider,
            LoggingSettings? loggingSettings = null)
        {
            this.jira = jira;
            this.metadataProvider = metadataProvider;
            this.loggingSettings = loggingSettings;
        }

        /// <summary>
        /// Creates a Linq Provider to build and execute requests to JIRA.
        /// </summary>
        /// <typeparam name="TIssue">Type of queryable issues. Has to implement <c>SimpleJira.Interface.Types.JiraIssue</c></typeparam>
        /// <returns>
        /// 	Queryable.
        /// </returns>
        public IQueryable<TIssue> GetIssues<TIssue>() where TIssue : JiraIssue
        {
            var stopwatch = loggingSettings.HasValue && loggingSettings.Value.Level <= LogLevel.Trace
                ? Stopwatch.StartNew()
                : null;
            var queryProvider = RelinqHelpers.CreateQueryProvider(metadataProvider, query => Execute(query, stopwatch));
            var scope = Scope.Get<TIssue>();
            return scope.Filter(new RelinqQueryable<TIssue>(queryProvider));
        }

        private IEnumerable Execute(BuiltQuery builtQuery, Stopwatch stopwatch)
        {
            if (builtQuery.IsAny.HasValue && builtQuery.IsAny.Value)
                return ExecuteAny(builtQuery, stopwatch);
            if (builtQuery.Count.HasValue && builtQuery.Count.Value)
                return ExecuteCount(builtQuery, stopwatch);
            return ExecuteCollection(builtQuery, stopwatch);
        }

        private IEnumerable ExecuteAny(BuiltQuery builtQuery, Stopwatch stopwatch)
        {
            var response = SelectIssuesInfo(builtQuery, stopwatch);
            yield return response.Total > 0;
        }

        private IEnumerable ExecuteCount(BuiltQuery builtQuery, Stopwatch stopwatch)
        {
            var response = SelectIssuesInfo(builtQuery, stopwatch);
            yield return response.Total;
        }

        private IEnumerable ExecuteCollection(BuiltQuery builtQuery, Stopwatch stopwatch)
        {
            var keys = new HashSet<string>();
            var fields = builtQuery.Projection?.fields
                .Select(x => x.Expression)
                .ToArray();
            var skip = builtQuery.Skip ?? 0;
            var take = builtQuery.Take ?? int.MaxValue;
            var projection = builtQuery.Projection != null
                ? ProjectionMapperFactory.GetMapper(builtQuery.Projection)
                : null;
            if (projection == null && loggingSettings.NeedLogging(LogLevel.Warning))
                if (warnings.TryAdd(Environment.StackTrace, 0))
                    loggingSettings.Log(LogLevel.Warning,
                        $"Query [{builtQuery.Query}] doesn't have .Select(...) construction. It may be critical to the performance because Jira should return all fields of issues.");

            loggingSettings.Log(LogLevel.Debug, $"Query [{builtQuery.Query}] is starting execution");

            int got;
            int maxResults;

            do
            {
                maxResults = Math.Min(take, packetSize);

                Stopwatch partialStopwatch = null;
                if (loggingSettings.NeedLogging(LogLevel.Trace))
                {
                    partialStopwatch = Stopwatch.StartNew();
                    loggingSettings.Log(LogLevel.Trace,
                        $"Query's part (startAt: [{skip}], maxResults: [{maxResults}], jql: [{builtQuery.Query}]) is starting execution");
                }

                var request = new JiraIssuesRequest
                {
                    Jql = builtQuery.Query?.ToString() ?? "",
                    StartAt = skip,
                    MaxResults = maxResults,
                    Fields = fields
                };
                JiraIssuesResponse response;
                try
                {
                    response = jira.SelectIssues(request, builtQuery.IssueType);
                    if (partialStopwatch != null)
                    {
                        partialStopwatch.Stop();
                        loggingSettings.Log(LogLevel.Trace,
                            $"Query's partial execution (startAt: [{skip}], maxResults: [{maxResults}], jql: [{builtQuery.Query}]) is finished. Returned [{response.Issues.Length}] issues, took [{partialStopwatch.Elapsed.TotalMilliseconds}] ms");
                    }
                }
                catch (Exception)
                {
                    if (partialStopwatch != null)
                    {
                        partialStopwatch.Stop();
                        loggingSettings.Log(LogLevel.Trace,
                            $"Query's partial execution (startAt: [{skip}], maxResults: [{maxResults}], jql: [{builtQuery.Query}]) is finished. Threw an exception, took [{partialStopwatch.Elapsed.TotalMilliseconds}] ms");
                    }

                    stopwatch?.Stop();

                    throw;
                }

                got = response.Issues.Length;
                for (var i = 0; i < response.Issues.Length && take > 0; ++i)
                {
                    var issue = response.Issues[i];
                    if (keys.Add(issue.Key))
                    {
                        yield return projection != null ? projection(issue) : issue;
                        take--;
                    }
                }

                skip += packetSize;
            } while (take > 0 && got == maxResults);

            if (stopwatch != null)
            {
                stopwatch.Stop();
                loggingSettings.Log(LogLevel.Trace,
                    $"Query's execution [{builtQuery.Query}] is finished. Returned [{keys.Count}] issues, took [{stopwatch.Elapsed.TotalMilliseconds}] ms");
            }
        }

        private JiraIssuesResponse SelectIssuesInfo(BuiltQuery builtQuery, Stopwatch stopwatch)
        {
            loggingSettings.Log(LogLevel.Debug,
                $"Getting the count of issues is starting execution, query [{builtQuery.Query}] ");

            try
            {
                var request = new JiraIssuesRequest
                {
                    Jql = builtQuery.Query?.ToString() ?? "",
                    StartAt = 0,
                    MaxResults = 1,
                    Fields = new[] {"created"}
                };

                var response = jira.SelectIssuesAsync(request, builtQuery.IssueType, CancellationToken.None)
                    .GetAwaiter().GetResult();
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    loggingSettings.Log(LogLevel.Trace,
                        $"Getting the count of issues is finished, query [{builtQuery.Query}], took [{stopwatch.Elapsed.TotalMilliseconds}] ms");
                }

                return response;
            }
            catch (Exception)
            {
                if (stopwatch != null)
                {
                    stopwatch.Stop();
                    loggingSettings.Log(LogLevel.Trace,
                        $"Getting the count of issues is finished, query [{builtQuery.Query}]. Threw an exception, took [{stopwatch.Elapsed.TotalMilliseconds}] ms");
                }

                throw;
            }
        }
    }
}