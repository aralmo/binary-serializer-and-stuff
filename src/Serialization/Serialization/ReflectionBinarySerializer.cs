using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

public class ReflectionBinarySerializer<TModel>
{
    SerializationSettings settings;
    public ReflectionBinarySerializer(SerializationSettings settings = null)
    {
        this.settings = settings ?? new SerializationSettings();
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        if (this.settings.SerializePrivates) bindingFlags |= BindingFlags.NonPublic;
        var pgetters = typeof(TModel)
            .GetProperties(bindingFlags)
            .Where(p => p.CanWrite && p.CanRead)
            .OrderBy(p => p.Name)
            .Select(TypeAndGetter)
            .Select(p => (serializer: SerializerFor(p.type), getter: p.getter))
            .ToArray();
    }

    public Action<object, Stream> SerializerFor(Type type)
    {
        switch (type)
        {
            case Type t when t == typeof(int):
                return (o, s) => s.Write(BitConverter.GetBytes((int)o), 0, 4);
            case Type t when t == typeof(uint):
                return (o, s) => s.Write(BitConverter.GetBytes((uint)o), 0, 4);
            case Type t when t == typeof(long):
                return (o, s) => s.Write(BitConverter.GetBytes((long)o), 0, 8);
            case Type t when t == typeof(ulong):
                return (o, s) => s.Write(BitConverter.GetBytes((ulong)o), 0, 8);
            case Type t when t == typeof(short):
                return (o, s) => s.Write(BitConverter.GetBytes((short)o), 0, 2);
            case Type t when t == typeof(ushort):
                return (o, s) => s.Write(BitConverter.GetBytes((ushort)o), 0, 2);
            case Type t when t == typeof(byte):
                return (o, s) => s.WriteByte((byte)o);
            case Type t when t == typeof(sbyte):
                return (o, s) => s.WriteByte((byte)((sbyte)o + byte.MaxValue + 1));
            case Type t when t == typeof(float):
                return (o, s) => s.Write(BitConverter.GetBytes((float)o), 0, 4);
            case Type t when t == typeof(double):
                return (o, s) => s.Write(BitConverter.GetBytes((double)o), 0, 8);
            case Type t when t == typeof(bool):
                return (o, s) => s.Write(BitConverter.GetBytes((bool)o), 0, 1);
            case Type t when t == typeof(char):
                return (o, s) => s.Write(BitConverter.GetBytes((char)o), 0, 2);
            case Type t when t == typeof(DateTime):
                return (o, s) => s.Write(BitConverter.GetBytes(((DateTime)o).ToBinary()), 0, 8);
            case Type t when t == typeof(TimeSpan):
                return (o, s) => s.Write(BitConverter.GetBytes(((TimeSpan)o).Ticks), 0, 8);
            case Type t when t == typeof(decimal):
                return (o, s) =>
                {
                    var bits = decimal.GetBits((decimal)o);
                    foreach (var bit in bits)
                    {
                        s.Write(BitConverter.GetBytes(bit), 0, 4);
                    }
                };
            case Type t when t == typeof(string):
                return (o, s) =>
                {
                    var v = o as string;
                    s.Write(BitConverter.GetBytes(v.Length), 0, 4);
                    var tbytes = settings.TextEncoding.GetBytes(v);
                    s.Write(tbytes, 0, tbytes.Length);
                };
            case Type t when t.IsArray && t.GetArrayRank() == 1:
                return (o, s) =>
                {
                    var itemSerializer = SerializerFor(t.GetElementType());
                    var iterator = (IEnumerable)o;
                    int count = 0;
                    foreach (var _ in iterator) count++;
                    s.Write(BitConverter.GetBytes(count), 0, 4);
                    foreach (var x in iterator)
                    {
                        itemSerializer(x, s);
                    }
                };
            default: throw new Exception("type not implemented");
        }
    }
    public Func<Stream, object> DeserializerFor(Type type)
    {
        switch (type)
        {
            case Type t when t == typeof(int):
                return s => BitConverter.ToInt32(ReadBytes(s, 4), 0);
            case Type t when t == typeof(long):
                return s => BitConverter.ToInt64(ReadBytes(s, 8), 0);
            case Type t when t == typeof(short):
                return s => BitConverter.ToInt16(ReadBytes(s, 2), 0);
            case Type t when t == typeof(uint):
                return s => BitConverter.ToUInt32(ReadBytes(s, 4), 0);
            case Type t when t == typeof(ulong):
                return s => BitConverter.ToUInt64(ReadBytes(s, 8), 0);
            case Type t when t == typeof(ushort):
                return s => BitConverter.ToUInt16(ReadBytes(s, 2), 0);           
            case Type t when t == typeof(byte):
                return s => (byte)s.ReadByte();
            case Type t when t == typeof(sbyte):
                return s => (sbyte)(s.ReadByte() - byte.MaxValue - 1);
            case Type t when t == typeof(float):
                return s => BitConverter.ToSingle(ReadBytes(s, 4), 0);
            case Type t when t == typeof(double):
                return s => BitConverter.ToDouble(ReadBytes(s, 8), 0);
            case Type t when t == typeof(bool):
                return s => BitConverter.ToBoolean(ReadBytes(s, 1), 0);
            case Type t when t == typeof(char):
                return s => BitConverter.ToChar(ReadBytes(s, 2), 0);
            case Type t when t == typeof(DateTime):
                return s => DateTime.FromBinary(BitConverter.ToInt64(ReadBytes(s, 8), 0));
            case Type t when t == typeof(TimeSpan):
                return s => new TimeSpan(BitConverter.ToInt64(ReadBytes(s, 8), 0));
            case Type t when t == typeof(decimal):
                return s =>
                {
                    var bits = new int[4];
                    for (int i = 0; i < 4; i++)
                    {
                        bits[i] = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    }
                    return new decimal(bits);
                };
            case Type t when t == typeof(string):
                return s =>
                {
                    var length = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    var stringBytes = ReadBytes(s, length);
                    return Encoding.UTF8.GetString(stringBytes);
                };
            case Type t when t.IsArray && t.GetArrayRank() == 1:
                return s =>
                {
                    var elementType = t.GetElementType();
                    var deserializer = DeserializerFor(elementType);
                    var count = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    var array = Array.CreateInstance(elementType, count);
                    for (int i = 0; i < count; i++)
                    {
                        array.SetValue(deserializer(s), i);
                    }
                    return array;
                };
            default:
                throw new Exception("Type not implemented");
        }

    }
    static byte[] ReadBytes(Stream stream, int count)
    {
        var bytes = new byte[count];
        stream.Read(bytes, 0, count);
        return bytes;
    }
    static (Type type, Func<TModel, object> getter) TypeAndGetter(PropertyInfo pinfo)
        => (pinfo.PropertyType, (TModel model) => pinfo.GetValue(model));
}
public class SerializationSettings
{
    public bool SerializePrivates;
    public Encoding TextEncoding = Encoding.UTF8;
}