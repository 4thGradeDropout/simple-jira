using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleJira.Impl.Serialization
{
    internal class JiraJsonConverter<TObject, TObjectDto> : JsonConverter<TObject>
    {
        private static readonly AutoMapper writeMapper = AutoMapper.Create<TObject, TObjectDto>();
        private static readonly AutoMapper readMapper = AutoMapper.Create<TObjectDto, TObject>();

        public override TObject Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var dto = JsonSerializer.Deserialize<TObjectDto>(ref reader, options);
            return (TObject)readMapper.Map(dto);
        }

        public override void Write(Utf8JsonWriter writer, TObject value, JsonSerializerOptions options)
        {
            var dto = writeMapper.Map(value);
            JsonSerializer.Serialize(writer, dto, options);
        }
    }
}