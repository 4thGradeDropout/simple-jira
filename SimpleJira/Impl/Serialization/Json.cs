using System;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SimpleJira.Impl.Dto;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl.Serialization
{
    internal static class Json
    {
        private static readonly JsonSerializer serializer = CreateJsonSerializer();

        public static string Serialize(object obj)
        {
            var stringWriter = new StringWriter(new StringBuilder(256), CultureInfo.InvariantCulture);
            using (var jsonTextWriter = new JsonTextWriter(stringWriter))
            {
                jsonTextWriter.Formatting = serializer.Formatting;
                serializer.Serialize(jsonTextWriter, obj, typeof(JObject));
            }

            return stringWriter.ToString();
        }

        public static T Deserialize<T>(string json)
        {
            using var jsonTextReader = new JsonTextReader(new StringReader(json));
            return (T) serializer.Deserialize(jsonTextReader, typeof(T));
        }

        public static object FromToken(object jObject, Type type)
        {
            if (jObject == null)
                return null;
            if (jObject is JToken jToken)
                return jToken.ToObject(type, serializer);
            return Convert.ChangeType(jObject, type);
        }

        public static object ToToken(object obj)
        {
            return JToken.FromObject(obj, serializer);
        }

        private static JsonSerializer CreateJsonSerializer()
        {
            var jsonSerializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            jsonSerializer.NullValueHandling = NullValueHandling.Ignore;
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraAttachment, JiraAttachmentDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraAvatarUrls, JiraAvatarUrlsDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraComment, JiraCommentDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraCustomFieldOption, JiraCustomFieldOptionDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraIssueReference, JiraIssueReferenceDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraIssueType, JiraIssueTypeDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraPriority, JiraPriorityDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraProject, JiraProjectDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraStatusCategory, JiraStatusCategoryDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraStatus, JiraStatusDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraUser, JiraUserDto>());
            jsonSerializer.Converters.Add(new JiraJsonConverter<JiraIssueComments, JiraIssueCommentsDto>());
            return jsonSerializer;
        }
    }
}