using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace DataToolChain.DbStringer;

public class DeflateHelper
{
    public static string CompressString(string text)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        using (var memoryStream = new MemoryStream())
        {
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
            {
                deflateStream.Write(buffer, 0, buffer.Length);
            }

            return Convert.ToBase64String(memoryStream.ToArray());
        }
    }

    public static string DecompressString(string compressedText)
    {
        byte[] gzBuffer = Convert.FromBase64String(compressedText);
        using (var memoryStream = new MemoryStream())
        {
            int msgLength = BitConverter.ToInt32(gzBuffer, 0);
            memoryStream.Write(gzBuffer, 4, gzBuffer.Length - 4);

            byte[] buffer = new byte[msgLength];

            memoryStream.Position = 0;
            using (var deflateStream = new DeflateStream(memoryStream, CompressionMode.Decompress))
            {
                deflateStream.Read(buffer, 0, buffer.Length);
            }

            return Encoding.UTF8.GetString(buffer);
        }
    }
}
