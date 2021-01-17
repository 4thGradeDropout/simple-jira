using System;
using System.Net.Http.Headers;
using System.Text;

namespace SimpleJira.Impl.RestApi
{
    internal static class BasicAuthenticationHeader
    {
        public static AuthenticationHeaderValue Value(string user, string password)
        {
            var value = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
            return new AuthenticationHeaderValue("Basic", value);
        }
    }
}