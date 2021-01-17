using System;
using System.Collections.Generic;

namespace SimpleJira.Interface.Metadata
{
    public interface IJiraMetadataProvider
    {
        IJiraIssueMetadataCollection Issues { get; }
    }

    public interface IJiraIssueMetadataCollection : IEnumerable<IJiraIssueMetadata>
    {
    }

    public interface IJiraIssueMetadata
    {
        Type Type { get; }
        object Workflow { get; }
        IJiraFieldMetadataCollection Fields { get; }
    }

    public interface IJiraFieldMetadataCollection : IEnumerable<IJiraFieldMetadata>
    {
    }
}