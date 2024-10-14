using System;
using System.IO;

public struct Fragment
{
    /// <summary>
    /// Free use fragment type
    /// </summary>
    public byte Type;
    public ulong Length;
}
public static partial class Serialize
{
    public static Span<byte> Fragment(Fragment fragment)
        => Fragment(fragment.Type, fragment.Length);
    public static Span<byte> Fragment(byte type, ulong length)
    {
        var lnn = NaturalNumber(length);
        var r = new byte[lnn.Length + 1];
        r[0] = type;
        var s = r.AsSpan(1);
        lnn.CopyTo(s);
        return r;
    }
}
public static partial class Deserialize
{
    public static Fragment Fragment(Stream stream)
    {
        var type = (byte)stream.ReadByte();
        var length = NaturalNumber(stream);
        return new Fragment { Type = type, Length = length };
    }
    public static Fragment Fragment(Span<byte> bytes)
    {
        var type = bytes[0];
        var length = NaturalNumber(bytes.Slice(1));
        return new Fragment { Type = type, Length = length };
    }
}