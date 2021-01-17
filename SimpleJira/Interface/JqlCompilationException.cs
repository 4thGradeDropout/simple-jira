using System;

namespace SimpleJira.Interface
{
    public class JqlCompilationException : Exception
    {
        public JqlCompilationException(string message) : base(message)
        {
        }
    }
}