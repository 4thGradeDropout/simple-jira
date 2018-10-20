using System;
using System.Net;
using System.Text;

namespace SimpleJira.Interface.RestApi
{
    public class JiraHttpTransportException : Exception
    {
        public JiraHttpTransportException(JiraEndPoint endPoint, HttpStatusCode httpStatusCode, byte[] body)
            : base(
                $"jira [{endPoint.Url}] returned a bad status code [{httpStatusCode}], body [{Encoding.UTF8.GetString(body)}]")
        {
        }
    }
}