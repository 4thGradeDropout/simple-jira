namespace SimpleJira.Interface.Types
{
    public class JiraTransition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public JiraStatus To { get; set; }
    }
}