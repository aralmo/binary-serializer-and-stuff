using FluentAssertions;

namespace SerializationTests;

public class NaturalNumbersSerialization
{
    [Fact]
    public void SerializeDeserialize()
        => Enumerable.Range(1, 9).Select(n => (ulong)(n / 10f * ulong.MaxValue))
            .Should().AllSatisfy(n =>
            {
                var bytes = Serialize.NaturalNumber(n);
                var stream = new MemoryStream(bytes.ToArray());
                Deserialize.NaturalNumber(bytes).Should().Be(n);
                Deserialize.NaturalNumber(stream).Should().Be(n);
            });
    [Fact]
    public void ShortNumbers()
    {
        Serialize.NaturalNumber(127).Length.Should().Be(1);
        Serialize.NaturalNumber(256).Length.Should().Be(2);
    }
    [Fact]
    public void MinMaxValues()
    {
        Deserialize.NaturalNumber(Serialize.NaturalNumber(ushort.MinValue)).Should().Be(ushort.MinValue);
        Deserialize.NaturalNumber(Serialize.NaturalNumber(ushort.MaxValue)).Should().Be(ushort.MaxValue);
    }
}