using System;
using System.IO;
using System.Text;

public static partial class Serialize
{
    public static byte[] String(string value)
    {
        var length = BitConverter.GetBytes(value.Length);
        var stringBytes = Encoding.UTF8.GetBytes(value);
        var result = new byte[length.Length + stringBytes.Length];
        Buffer.BlockCopy(length, 0, result, 0, length.Length);
        Buffer.BlockCopy(stringBytes, 0, result, length.Length, stringBytes.Length);
        return result;
    }
}

public static partial class Deserialize
{
    public static string String(Span<byte> span)
    {
        var b = span.ToArray();
        var length = BitConverter.ToInt32(b,0);
        if (length == 0) return string.Empty;
        return Encoding.UTF8.GetString(b,4,length);
    }

    public static string String(Stream stream)
    {
        var lengthBytes = new byte[4];
        stream.Read(lengthBytes, 0, 4);
        var length = BitConverter.ToInt32(lengthBytes, 0);

        var stringBytes = new byte[length];
        stream.Read(stringBytes, 0, length);
        return Encoding.UTF8.GetString(stringBytes);
    }
}
