using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

public partial class BinarySerializer<TModel>
{
    SerializationSettings settings;
    static readonly Dictionary<Type, PropertyInfo[]> typesCache = new Dictionary<Type, PropertyInfo[]>();
    public BinarySerializer(SerializationSettings settings = null)
    {
        this.settings = settings ?? new SerializationSettings();
    }
    private PropertyInfo[] GetTypeInfo(Type t)
    {
        var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
        if (settings.SerializePrivates) bindingFlags |= BindingFlags.NonPublic;
        if (!typesCache.TryGetValue(t, out var properties))
        {
            properties = t.GetProperties(bindingFlags)
                           .Where(p => p.CanWrite && p.CanRead)
                           .OrderBy(p => p.Name)
                           .ToArray();
            typesCache.Add(t, properties);
        }
        return properties;
    }
    static byte[] ReadBytes(Stream stream, int count)
    {
        var bytes = new byte[count];
        stream.Read(bytes, 0, count);
        return bytes;
    }
    public void Serialize(TModel model, Stream stream)
    {
        GetClassSerializer(typeof(TModel))(model, stream);
    }
    public TModel Deserialize(Stream stream)
    {
        return (TModel) GetClassDeserializer(typeof(TModel))(stream);
    }
}
public class SerializationSettings
{
    public bool SerializePrivates;
    public Encoding TextEncoding = Encoding.UTF8;
}