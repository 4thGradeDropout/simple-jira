using SimpleJira.Fakes.Impl;
using SimpleJira.Interface.Metadata;
using SimpleJira.Interface.Types;

namespace SimpleJira.Fakes.Interface
{
    public static class FakeJira
    {
        public static IMockJira InMemory(string fakeHostUrl, JiraUser authorizedUser,
            IJiraMetadataProvider metadataProvider)
        {
            return new MockJira(fakeHostUrl, authorizedUser, new InMemoryJiraIssueStore(), metadataProvider);
        }

        public static IMockJira File(string folderPath, string fakeHostUrl, JiraUser authorizedUser,
            IJiraMetadataProvider metadataProvider)
        {
            return new MockJira(fakeHostUrl, authorizedUser, new FileJiraIssueStore(folderPath), metadataProvider);
        }
    }
}