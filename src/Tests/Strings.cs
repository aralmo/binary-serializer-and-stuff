using FluentAssertions;

namespace SerializationTests;

public class StringSerialization
{
    [Theory]
    [InlineData("Hello")]
    [InlineData("World")]
    [InlineData("Serialization")]
    [InlineData("Test")]
    public void SerializeDeserialize(string str)
    {
        var bytes = Serialize.String(str).AsSpan();
        var stream = new MemoryStream(bytes.ToArray());
        Deserialize.String(bytes).Should().Be(str);
        Deserialize.String(stream).Should().Be(str);
    }

    [Fact]
    public void EmptyString()
    {
        var str = "";
        var bytes = Serialize.String(str);
        bytes.Length.Should().Be(4); // Only length bytes
    }
}
