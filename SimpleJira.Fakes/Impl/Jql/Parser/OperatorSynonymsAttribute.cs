using System;

namespace SimpleJira.Fakes.Impl.Jql.Parser
{
    internal class OperatorSynonymsAttribute : Attribute
    {
        public OperatorSynonymsAttribute(params string[] synonyms)
        {
            Synonyms = synonyms;
        }

        public string[] Synonyms { get; }
    }
}