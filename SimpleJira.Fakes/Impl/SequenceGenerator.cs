using System;
using System.IO;

namespace SimpleJira.Fakes.Impl
{
    internal class SequenceGenerator
    {
        private readonly string folderPath;
        private readonly string sequencePath;

        private const int longSize = sizeof(long);

        public SequenceGenerator(string folderPath, string sequenceFileName)
        {
            this.folderPath = folderPath;
            sequencePath = Path.Combine(folderPath, sequenceFileName);
        }

        public long NextValue()
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            var buffer = new byte[longSize];
            long value = 0;
            using (var stream = new FileStream(sequencePath, FileMode.OpenOrCreate, FileAccess.Read))
            {
                if (stream.Length > 0)
                {
                    stream.ReadExactly(buffer, 0, longSize);
                    value = BitConverter.ToInt64(buffer, 0);
                }
            }

            using (var stream = new FileStream(sequencePath, FileMode.Open, FileAccess.Write))
            {
                buffer = BitConverter.GetBytes(++value);
                stream.Write(buffer, 0, longSize);
            }

            return value;
        }

        public void Drop()
        {
            if (Directory.Exists(folderPath))
                if (File.Exists(sequencePath))
                    File.Delete(sequencePath);
        }
    }
}