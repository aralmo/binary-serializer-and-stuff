namespace Serialization;
public class GenericEnumeratorsSerializationTests
{
    [Theory]
    [MemberData(nameof(GetIEnumerableTestData))]
    public void SerializeDeserialize_IEnumerable<T>(IEnumerable<T> value)
    {
        var serializer = new BinarySerializer<IEnumerable<T>>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(IEnumerable<T>))(value, stream);
        stream.Position = 0;
        var deserializedValue = (IEnumerable<T>)serializer.DeserializerFor(typeof(IEnumerable<T>))(stream);
        Assert.Equal(value, deserializedValue);
    }

    public static IEnumerable<object[]> GetIEnumerableTestData()
    {
        yield return new object[] { new List<int> { 1, 2, 3 } };
        yield return new object[] { new List<string> { "a", "b", "c" } };
        yield return new object[] { new List<double> { 1.1, 2.2, 3.3 } };
    }

    [Theory]
    [MemberData(nameof(GetListTestData))]
    public void SerializeDeserialize_List<T>(List<T> value)
    {
        var serializer = new BinarySerializer<List<T>>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(List<T>))(value, stream);
        stream.Position = 0;
        var deserializedValue = (List<T>)serializer.DeserializerFor(typeof(List<T>))(stream);
        Assert.Equal(value, deserializedValue);
    }

    public static IEnumerable<object[]> GetListTestData()
    {
        yield return new object[] { new List<int> { 4, 5, 6 } };
        yield return new object[] { new List<string> { "d", "e", "f" } };
        yield return new object[] { new List<double> { 4.4, 5.5, 6.6 } };
    }

    [Fact]
    public void SerializeDeserialize_Dictionary()
    {
        var serializer = new BinarySerializer<Dictionary<string, string>>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(Dictionary<string, string>))(new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } }, stream);
        stream.Position = 0;
        var deserializedValue = (Dictionary<string, string>)serializer.DeserializerFor(typeof(Dictionary<string, string>))(stream);
        Assert.Equal(new Dictionary<string, string> { { "key1", "value1" }, { "key2", "value2" }, { "key3", "value3" } }, deserializedValue);
    }

}