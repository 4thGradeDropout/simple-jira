using System;
using System.Collections.Generic;
using System.IO;

namespace SimpleJira.Fakes.Impl
{
    internal class BlobStorage
    {
        private readonly string folderPath;
        private readonly string contentFilePath;
        private readonly string indexFilePath;

        private const int longSize = sizeof(long);
        private const int boolSize = sizeof(bool);
        private const int intSize = sizeof(int);
        private const int indexItemSize = longSize + boolSize + longSize + intSize;
        private static readonly int indexReadingBufferSize = Environment.SystemPageSize / indexItemSize * indexItemSize;

        private static readonly Comparison<(long key, bool deleted, long position, int length)> comparison =
            (t1, t2) => t1.position.CompareTo(t2.position);

        public BlobStorage(string folderPath, string contentFile, string indexFile)
        {
            this.folderPath = folderPath;
            contentFilePath = Path.Combine(folderPath, contentFile);
            indexFilePath = Path.Combine(folderPath, indexFile);
        }

        public void Write(long key, byte[] bytes)
        {
            Write(key, false, bytes);
        }

        public bool TryDelete(long key)
        {
            var index = ReadIndex();
            if (index == null || !index.TryGetValue(key, out var tuple) || tuple.deleted)
                return false;
            Write(key, true, new byte[0]);
            return true;
        }

        public bool TryRead(long key, out byte[] bytes)
        {
            var index = ReadIndex();
            if (index == null || !index.TryGetValue(key, out var tuple) || tuple.deleted)
            {
                bytes = null;
                return false;
            }

            using var file = new FileStream(contentFilePath, FileMode.Open, FileAccess.Read);
            file.Seek(tuple.position, SeekOrigin.Begin);
            bytes = new byte[tuple.length];
            file.ReadExactly(bytes, 0, tuple.length);
            return true;
        }

        public IEnumerable<KeyValuePair<long, byte[]>> ReadAll()
        {
            var index = ReadIndex();
            if (index == null || index.Count == 0)
                yield break;
            var tuples = new (long key, bool deleted, long position, int length)[index.Count];
            int i = 0;
            foreach (var item in index)
            {
                tuples[i] = (item.Key, item.Value.deleted, item.Value.position, item.Value.length);
                ++i;
            }

            Array.Sort(tuples, comparison);
            using var file = new FileStream(contentFilePath, FileMode.Open, FileAccess.Read);
            foreach (var (key, deleted, position, length) in tuples)
            {
                if (deleted)
                    continue;
                var shift = position - file.Position;
                if (shift != 0)
                    file.Seek(shift, SeekOrigin.Current);
                var bytes = new byte[length];
                file.ReadExactly(bytes);
                yield return new KeyValuePair<long, byte[]>(key, bytes);
            }
        }

        public void Drop()
        {
            if (Directory.Exists(folderPath))
            {
                if (File.Exists(indexFilePath))
                    File.Delete(indexFilePath);
                if (File.Exists(contentFilePath))
                    File.Delete(contentFilePath);
            }
        }

        private Dictionary<long, (bool deleted, long position, int length)> ReadIndex()
        {
            if (!File.Exists(indexFilePath))
                return null;
            var result = new Dictionary<long, (bool deleted, long position, int length)>();
            var fileReadingBuffer = new byte[indexReadingBufferSize];
            using (var file = new FileStream(indexFilePath, FileMode.Open, FileAccess.Read))
            {
                int read;
                do
                {
                    read = file.Read(fileReadingBuffer, 0, indexReadingBufferSize);
                    var bufferOffset = 0;
                    while (bufferOffset < read)
                    {
                        var key = BitConverter.ToInt64(fileReadingBuffer, bufferOffset);
                        bufferOffset += longSize;
                        var deleted = BitConverter.ToBoolean(fileReadingBuffer, bufferOffset);
                        bufferOffset += boolSize;
                        var position = BitConverter.ToInt64(fileReadingBuffer, bufferOffset);
                        bufferOffset += longSize;
                        var length = BitConverter.ToInt32(fileReadingBuffer, bufferOffset);
                        bufferOffset += intSize;
                        result[key] = (deleted, position, length);
                    }
                } while (read == indexReadingBufferSize);
            }

            return result;
        }

        private void Write(long key, bool deleted, byte[] bytes)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            long position;
            using (var file = new FileStream(contentFilePath, FileMode.Append, FileAccess.Write))
            {
                position = file.Length;
                if (bytes.Length > 0) file.Write(bytes, 0, bytes.Length);
            }

            using var index = new FileStream(indexFilePath, FileMode.Append, FileAccess.Write);
            index.Write(BitConverter.GetBytes(key), 0, longSize);
            index.Write(BitConverter.GetBytes(deleted), 0, boolSize);
            index.Write(BitConverter.GetBytes(position), 0, longSize);
            index.Write(BitConverter.GetBytes(bytes.Length), 0, intSize);
        }
    }
}