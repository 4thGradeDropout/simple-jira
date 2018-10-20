using SimpleJira.Impl.Mock;
using SimpleJira.Impl.Mock.InMemory;
using SimpleJira.Impl.RestApi;
using SimpleJira.Interface.ObjectModel;
using SimpleJira.Interface.RestApi;

namespace SimpleJira.Interface
{
    public static class JiraHost
    {
        public static IJiraHost RestApi(JiraEndPoint endPoint)
        {
            return new RestApiJiraHost(endPoint);
        }

        public static IJiraHost InMemory(JiraMetadata metadata, string keyPrefix)
        {
            return new MockJiraHost(metadata, new InMemoryJiraIssueMockStore(keyPrefix));
        }
    }
}