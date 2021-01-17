using System;

namespace SimpleJira.Interface
{
    public class JiraException : Exception
    {
        public JiraException(string message) : base(message)
        {
        }
    }
}