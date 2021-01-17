using SimpleJira.Interface;

namespace SimpleJira.Fakes.Interface
{
    public interface IMockJira : IJira
    {
        void Drop();
    }
}