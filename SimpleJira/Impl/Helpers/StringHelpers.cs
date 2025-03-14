using SimpleJira.Impl.Serialization;

namespace SimpleJira.Impl.Helpers
{
    internal static class StringHelpers
    {
        public static string Escape(string source)
        {
            return Json.Serialize(source);
        }

        public static string Unescape(string source)
        {
            return Json.Deserialize<string>(source);
        }
    }
}