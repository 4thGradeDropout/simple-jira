using System.Reflection;

namespace SimpleJira.Impl.Queryable
{
    internal class Projection
    {
        public QueryField[] fields;
        public SelectedProperty[] properties;
        public ConstructorInfo ctor;
        public SelectedProperty[] ctorProperties;
        public MemberInfo[] initMembers;
    }
}