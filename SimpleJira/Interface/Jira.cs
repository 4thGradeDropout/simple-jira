using SimpleJira.Impl.RestApi;

namespace SimpleJira.Interface
{
    public static class Jira
    {
        /// <summary>
        /// Creates a proxy to JIRA REST API v2.
        /// </summary>
        /// <param name="hostUrl">JIRA URL.</param>
        /// <param name="user">Username.</param>
        /// <param name="password">Password.</param>
        /// <returns>
        /// 	proxy to JIRA REST API v2.
        /// </returns>
        public static IJira RestApi(string hostUrl, string user, string password)
        {
            return new RestApiJira(hostUrl, user, password);
        }
    }
}