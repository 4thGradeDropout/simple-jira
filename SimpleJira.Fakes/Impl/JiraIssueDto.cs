using System;
using System.IO;
using System.Runtime.Serialization;
using ProtoBuf;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Fakes.Impl
{
    internal class JiraIssueDto
    {
        public string Key { get; set; }
        public string Id { get; set; }
        public string Self { get; set; }
        public JiraIssueFields IssueFields { get; set; }

        public byte[] ToBytes()
        {
            var model = new JiraIssueDtoBinary
            {
                key = Key,
                id = Id,
                self = Self,
                json = IssueFields.ToJson()
            };
            using var ms = new MemoryStream();
            Serializer.Serialize(ms, model);
            return ms.ToArray();
        }

        public static JiraIssueDto FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var model = Serializer.Deserialize<JiraIssueDtoBinary>(ms);
            return new JiraIssueDto
            {
                Key = model.key,
                Id = model.id,
                Self = model.self,
                IssueFields = JiraIssueFields.FromJson(model.json)
            };
        }

        [ProtoContract]
        private class JiraIssueDtoBinary : ISerializable
        {
            [ProtoMember(1)]
            public string key;
            [ProtoMember(2)]
            public string id;
            [ProtoMember(3)]
            public string self;
            [ProtoMember(4)]
            public string json;

            private static readonly Type stringType = typeof(string);

            public JiraIssueDtoBinary()
            {
            }

            protected JiraIssueDtoBinary(
                SerializationInfo info,
                StreamingContext context)
            {
                key = (string) info.GetValue(nameof(key), stringType);
                id = (string) info.GetValue(nameof(id), stringType);
                self = (string) info.GetValue(nameof(self), stringType);
                json = (string) info.GetValue(nameof(json), stringType);
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue(nameof(key), key);
                info.AddValue(nameof(id), id);
                info.AddValue(nameof(self), self);
                info.AddValue(nameof(json), json);
            }
        }
    }
}