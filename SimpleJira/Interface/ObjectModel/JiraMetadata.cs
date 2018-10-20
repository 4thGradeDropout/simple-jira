namespace SimpleJira.Interface.ObjectModel
{
    public class JiraMetadata
    {
        private readonly JiraProject[] projects;

        public JiraMetadata(JiraProject[] projects)
        {
            this.projects = projects;
        }

        public JiraProject[] GetProjects()
        {
            return projects;
        }
    }
}