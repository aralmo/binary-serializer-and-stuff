using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

public partial class BinarySerializer<TModel>
{
    static readonly Dictionary<Type, Func<Stream, object>> classDeserializersCache = new Dictionary<Type, Func<Stream, object>>();
    static readonly Dictionary<Type, Func<Stream, object>> abstractClassDeserializersCache = new Dictionary<Type, Func<Stream, object>>();
    Func<Stream, object> GetClassDeserializer(Type type)
    {
        if (!classDeserializersCache.TryGetValue(type, out var deserializer))
        {
            deserializer = CreateClassDeserializer(type);
            classDeserializersCache.Add(type, deserializer);
        }
        return deserializer;
    }
    Func<Stream, object> CreateClassDeserializer(Type type)
    {
        return stream =>
        {
            var obj = Activator.CreateInstance(type);
            var properties = GetTypeInfo(type);
            foreach (var property in properties)
            {
                var deserializer = DeserializerFor(property.PropertyType);
                var value = deserializer(stream);
                property.SetValue(obj, value);
            }
            return obj;
        };
    }
    Func<Stream, object> GetAbstractClassDeserializer(Type type)
    {
        if (!abstractClassDeserializersCache.TryGetValue(type, out var deserializer))
        {
            deserializer = CreateAbstractClassDeserializer(type);
            abstractClassDeserializersCache.Add(type, deserializer);
        }
        return deserializer;
    }
    //todo: improve performance here
    Func<Stream, object> CreateAbstractClassDeserializer(Type type)
    {
        return stream =>
        {
            var nameLength = BitConverter.ToInt32(ReadBytes(stream, 4), 0);
            var nameBytes = ReadBytes(stream, nameLength);
            var assemblyQualifiedName = Encoding.UTF8.GetString(nameBytes);
            var resolvedType = Type.GetType(assemblyQualifiedName);
            if (resolvedType == null)
            {
                throw new InvalidOperationException($"Could not resolve type: {assemblyQualifiedName}");
            }

            var obj = Activator.CreateInstance(resolvedType);
            var properties = GetTypeInfo(resolvedType);
            foreach (var property in properties)
            {
                var deserializer = DeserializerFor(property.PropertyType);
                var value = deserializer(stream);
                property.SetValue(obj, value);
            }
            return obj;
        };
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
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>):
                return s =>
                {
                    var elementType = t.GetGenericArguments()[0];
                    var deserializer = DeserializerFor(elementType);
                    var count = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    var listType = typeof(List<>).MakeGenericType(elementType);
                    var list = (IList)Activator.CreateInstance(listType);
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(deserializer(s));
                    }
                    return list;
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
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>):
                return s =>
                {
                    var keyType = t.GetGenericArguments()[0];
                    var valueType = t.GetGenericArguments()[1];
                    var keyDeserializer = DeserializerFor(keyType);
                    var valueDeserializer = DeserializerFor(valueType);
                    var count = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    var dictionaryType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                    var dictionary = (IDictionary)Activator.CreateInstance(dictionaryType);
                    for (int i = 0; i < count; i++)
                    {
                        var key = keyDeserializer(s);
                        var value = valueDeserializer(s);
                        dictionary.Add(key, value);
                    }
                    return dictionary;
                };
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>):
                return s =>
                {
                    var elementType = t.GetGenericArguments()[0];
                    var deserializer = DeserializerFor(elementType);
                    var count = BitConverter.ToInt32(ReadBytes(s, 4), 0);
                    var array = Array.CreateInstance(elementType, count);
                    for (int i = 0; i < count; i++)
                    {
                        array.SetValue(deserializer(s), i);
                    }
                    return array;
                };
            case Type t when t.IsClass:
                return t.IsAbstract
                ? GetAbstractClassDeserializer(t)
                : GetClassDeserializer(t);
            default:
                throw new Exception("Type not implemented");
        }

    }
}