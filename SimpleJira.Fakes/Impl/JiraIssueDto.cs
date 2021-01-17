using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using SimpleJira.Interface.Issue;

namespace SimpleJira.Fakes.Impl
{
    internal class JiraIssueDto
    {
        private static readonly BinaryFormatter formatter = new BinaryFormatter();
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
            formatter.Serialize(ms, model);
            return ms.ToArray();
        }

        public static JiraIssueDto FromBytes(byte[] bytes)
        {
            using var ms = new MemoryStream(bytes);
            var model = (JiraIssueDtoBinary) formatter.Deserialize(ms);
            return new JiraIssueDto
            {
                Key = model.key,
                Id = model.id,
                Self = model.self,
                IssueFields = JiraIssueFields.FromJson(model.json)
            };
        }

        [Serializable]
        private class JiraIssueDtoBinary : ISerializable
        {
            public string key;
            public string id;
            public string self;
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