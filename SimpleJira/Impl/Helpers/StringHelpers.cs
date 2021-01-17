using Newtonsoft.Json;

namespace SimpleJira.Impl.Helpers
{
    internal static class StringHelpers
    {
        public static string Escape(string source)
        {
            return JsonConvert.SerializeObject(source);
        }

        public static string Unescape(string source)
        {
            return JsonConvert.DeserializeObject<string>(source);
        }
    }
}