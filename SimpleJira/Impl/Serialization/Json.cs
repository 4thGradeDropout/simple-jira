using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using SimpleJira.Impl.Dto;
using SimpleJira.Interface.Types;

namespace SimpleJira.Impl.Serialization
{
    internal static class Json
    {
        private static readonly JsonSerializerOptions serializerOptions = CreateJsonSerializer();

        public static string Serialize(object obj)
        {
            return JsonSerializer.Serialize(obj, serializerOptions);
        }

        public static T Deserialize<T>(string json)
        {
            return JsonSerializer.Deserialize<T>(json, serializerOptions);
        }

        public static object FromToken(JsonNode jsonNode, Type type)
        {
            if (jsonNode == null)
                return null;

            type = Nullable.GetUnderlyingType(type) ?? type;

            if (jsonNode.GetType() == type)
                return jsonNode;

            return jsonNode.Deserialize(type, serializerOptions);
        }

        public static JsonNode ToToken(object obj)
        {
            return JsonSerializer.SerializeToNode(obj, serializerOptions);
        }

        private static JsonSerializerOptions CreateJsonSerializer()
        {
            var jsonSerializerOptions = new JsonSerializerOptions
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            jsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraAttachment, JiraAttachmentDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraAvatarUrls, JiraAvatarUrlsDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraComment, JiraCommentDto>());
            jsonSerializerOptions.Converters.Add(
                new JiraJsonConverter<JiraCustomFieldOption, JiraCustomFieldOptionDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraIssueReference, JiraIssueReferenceDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraIssueType, JiraIssueTypeDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraPriority, JiraPriorityDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraProject, JiraProjectDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraStatusCategory, JiraStatusCategoryDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraStatus, JiraStatusDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraUser, JiraUserDto>());
            jsonSerializerOptions.Converters.Add(new JiraJsonConverter<JiraIssueComments, JiraIssueCommentsDto>());
            return jsonSerializerOptions;
        }
    }
}