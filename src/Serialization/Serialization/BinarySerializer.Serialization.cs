using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public partial class BinarySerializer<TModel>
{
    static readonly Dictionary<Type, Action<object, Stream>> classSerializersCache = new Dictionary<Type, Action<object, Stream>>();
    static readonly Dictionary<Type, Action<object, Stream>> abstractClassSerializersCache = new Dictionary<Type, Action<object, Stream>>();
    Action<object, Stream> GetClassSerializer(Type type)
    {
        if (!classSerializersCache.TryGetValue(type, out var serializer))
        {
            serializer = CreateClassSerializer(type);
            classSerializersCache.Add(type, serializer);
        }
        return serializer;
    }
    Action<object, Stream> CreateClassSerializer(Type type)
    {
        return (obj, stream) =>
        {
            var properties = GetTypeInfo(type);
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                var serializer = SerializerFor(property.PropertyType);
                serializer(value, stream);
            }
        };
    }
    //todo: improve performance here
    Action<object, Stream> GetAbstractClassSerializer(Type type)
    {
        if (!abstractClassSerializersCache.TryGetValue(type, out var serializer))
        {
            serializer = CreateClassAbstractSerializer(type);
            abstractClassSerializersCache.Add(type, serializer);
        }
        return serializer;
    }

    Action<object, Stream> CreateClassAbstractSerializer(Type type)
    {
        return (obj, stream) =>
        {
            var otype = obj.GetType();
            var assemblyQualifiedName = otype.AssemblyQualifiedName;
            var nameBytes = Encoding.UTF8.GetBytes(assemblyQualifiedName);
            stream.Write(BitConverter.GetBytes(nameBytes.Length), 0, 4);
            stream.Write(nameBytes, 0, nameBytes.Length);

            var properties = GetTypeInfo(otype);
            foreach (var property in properties)
            {
                var value = property.GetValue(obj);
                var serializer = SerializerFor(property.PropertyType);
                serializer(value, stream);
            }
        };
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
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>):
                return (o, s) =>
                {
                    var itemType = t.GetGenericArguments()[0];
                    var itemSerializer = SerializerFor(itemType);
                    var list = (IList)o;
                    s.Write(BitConverter.GetBytes(list.Count), 0, 4);
                    foreach (var item in list)
                    {
                        itemSerializer(item, s);
                    }
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
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Dictionary<,>):
                return (o, s) =>
                {
                    var keyType = t.GetGenericArguments()[0];
                    var valueType = t.GetGenericArguments()[1];
                    var keySerializer = SerializerFor(keyType);
                    var valueSerializer = SerializerFor(valueType);
                    var dictionary = (IDictionary)o;
                    s.Write(BitConverter.GetBytes(dictionary.Count), 0, 4);
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        keySerializer(entry.Key, s);
                        valueSerializer(entry.Value, s);
                    }
                };
            case Type t when t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>):
                return (o, s) =>
                {
                    var itemType = t.GetGenericArguments()[0];
                    var itemSerializer = SerializerFor(itemType);
                    var enumerable = (IEnumerable)o;
                    var list = enumerable.Cast<object>().ToList(); // Convert IEnumerable to List to count items
                    s.Write(BitConverter.GetBytes(list.Count), 0, 4);
                    foreach (var item in list)
                    {
                        itemSerializer(item, s);
                    }
                };
            case Type t when t.IsClass:
                return t.IsAbstract
                    ? GetAbstractClassSerializer(t)
                    : GetClassSerializer(t);
            default: throw new Exception("type not implemented");
        }
    }
}