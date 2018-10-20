using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SimpleJira.Impl.Utilities
{
    internal static class DictionaryHelpers
    {
        public static Dictionary<string, object> ToDictionary(this Dictionary<string, object> dictionary)
        {
            return (Dictionary<string, object>) ToDictionary((object) dictionary);
        }

        private static object ToDictionary(object obj)
        {
            if (obj == null)
                return null;
            if (obj is Dictionary<string, object> dictionary)
                return dictionary.ToDictionary(x => x.Key, x => ToDictionary(x.Value));
            if (obj is JObject jObject)
                return jObject.Properties().ToDictionary(x => x.Name, x => ToDictionary(x.Value));
            if (obj is object[] array)
                return array.Select(ToDictionary).ToArray();
            if (obj is JArray jArray)
                return jArray.Select(ToDictionary).ToArray();
            if (obj is JValue jValue)
                return jValue.Value;
            return obj;
        }
    }
}