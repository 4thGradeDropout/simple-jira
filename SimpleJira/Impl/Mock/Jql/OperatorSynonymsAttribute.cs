using System;

namespace SimpleJira.Impl.Mock.Jql
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