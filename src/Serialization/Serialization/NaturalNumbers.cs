using System;
using System.Buffers;
using System.IO;

public static partial class Serialize
{
    /// <summary>
    /// Serializes a variable length natural number
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    public static Span<byte> NaturalNumber(ulong number)
    {
        var buffer = ArrayPool<byte>.Shared.Rent(10);
        var index = 0;
        do
        {
            var byteVal = number & 0x7F;
            number >>= 7;
            if (number != 0)
            {
                byteVal |= 0x80;
            }
            buffer[index++] = (byte)byteVal;
        } while (number != 0);
        var r = new byte[index];
        new Span<byte>(buffer, 0, index).CopyTo(r);
        ArrayPool<byte>.Shared.Return(buffer);
        return r;
    }
}

public static partial class Deserialize
{
    /// <summary>
    /// Deserialize a variable length natural number
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    public static ulong NaturalNumber(Stream stream)
    {
        ulong number = 0;
        int shift = 0;
        byte b;
        do
        {
            b = (byte)stream.ReadByte();
            number |= (ulong)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return number;
    }
    public static ulong NaturalNumber(Span<byte> bytes)
    {
        ulong number = 0;
        int shift = 0;
        int index = 0;
        byte b;
        do
        {
            b = bytes[index++];
            number |= (ulong)(b & 0x7F) << shift;
            shift += 7;
        } while ((b & 0x80) != 0);
        return number;
    }
}