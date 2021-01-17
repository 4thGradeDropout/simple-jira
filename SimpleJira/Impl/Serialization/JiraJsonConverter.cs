using System;
using Newtonsoft.Json;

namespace SimpleJira.Impl.Serialization
{
    internal class JiraJsonConverter<TObject, TObjectDto> : JsonConverter<TObject>
    {
        private static readonly AutoMapper writeMapper = AutoMapper.Create<TObject, TObjectDto>();
        private static readonly AutoMapper readMapper = AutoMapper.Create<TObjectDto, TObject>();
        public override void WriteJson(JsonWriter writer, TObject value, JsonSerializer serializer)
        {
            var dto = writeMapper.Map(value);
            serializer.Serialize(writer, dto);
        }

        public override TObject ReadJson(JsonReader reader, Type objectType, TObject existingValue,
            bool hasExistingValue,
            JsonSerializer serializer)
        {
            if (hasExistingValue)
                return existingValue;
            var dto = serializer.Deserialize<TObjectDto>(reader);
            return (TObject) readMapper.Map(dto);
        }
    }
}