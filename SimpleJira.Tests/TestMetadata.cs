using SimpleJira.Interface.Types;

namespace SimpleJira.Tests
{
    public static class TestMetadata
    {
        public static JiraProject Project => new JiraProject
        {
            Id = "1",
            Key = "TESTPROJECT",
            Name = "Тестовый проект",
            ProjectTypeKey = "typekey",
            Self = "https://jira.int/rest/api/2/project/1",
            AvatarUrls = new JiraAvatarUrls
            {
                Size16x16 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13304",
                Size24x24 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13305",
                Size32x32 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13306",
                Size48x48 = "https://jira.int/secure/projectavatar?size=xsmall&pid=1&avatarId=13307",
            }
        };

        public static JiraStatus Status => new JiraStatus
        {
            Self = "https://jira.knopka.com/rest/api/2/status/10029",
            Description = "",
            IconUrl = "https://jira.knopka.com/images/icons/statuses/generic.png",
            Name = "Мой статус",
            Id = "10029",
            StatusCategory = StatusCategory
        };

        public static JiraStatusCategory StatusCategory => new JiraStatusCategory
        {
            Key = "new",
            Id = 2,
            Name = "К выполнению",
            Self = "https://jira.int/rest/api/2/statuscategory/2",
            ColorName = "blue-gray"
        };

        public static JiraCustomFieldOption CustomFieldOption => new JiraCustomFieldOption
        {
            Id = "12364",
            Value = "Да",
            Self = "https://jira.int/rest/api/2/customfield/12364"
        };

        public static JiraPriority Priority => new JiraPriority
        {
            Id = "5425",
            Name = "High",
            Self = "https://jira.int/rest/api/2/priority/5425"
        };

        public static JiraUser User => new JiraUser
        {
            Active = true,
            Key = "user.key",
            Name = "user.name",
            Self = "https://jira.int/rest/api/2/user/user.key",
            AvatarUrls = new JiraAvatarUrls(),
            DisplayName = "Peter Smith",
            EmailAddress = "peter@jira.int",
            TimeZone = "some/zone"
        };

        public static JiraIssueReference Reference => new JiraIssueReference
        {
            Key = $"{TestMetadata.Project.Key}-12349",
            Self = $"https://jira.int/rest/api/2/issue/{TestMetadata.Project.Key}-12349",
            Id = "2324256",
        };

        public static JiraIssueType IssueType => new JiraIssueType
        {
            Description = "Question description",
            Id = "1242145",
            Name = "Question"
        };
    }
}