using FluentAssertions;
using System;

namespace Serialization;

public class FragmentSerialization
{
    [Theory]
    [InlineData(1, ulong.MinValue)]
    [InlineData(2, 200)]
    [InlineData(3, 300)]
    [InlineData(4, ulong.MaxValue)]
    public void SerializeDeserialize(byte type, ulong length)
    {
        var fragment = new Fragment { Type = type, Length = length };
        var bytes = Serialize.Fragment(fragment);
        var stream = new MemoryStream(bytes.ToArray());
        var deserializedFragment = Deserialize.Fragment(bytes);
        deserializedFragment.Type.Should().Be(type);
        deserializedFragment.Length.Should().Be(length);
        deserializedFragment = Deserialize.Fragment(stream);
        deserializedFragment.Type.Should().Be(type);
        deserializedFragment.Length.Should().Be(length);
    }

    [Fact]
    public void EmptyFragment()
    {
        var fragment = new Fragment { Type = 0, Length = 0 };
        var bytes = Serialize.Fragment(fragment);
        bytes.Length.Should().Be(2); // Only type byte
    }
}
