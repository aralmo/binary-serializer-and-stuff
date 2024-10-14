using System.Collections;
namespace Generator;

public class SerializerTests
{
    [Theory]
    [InlineData(42)]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    [InlineData(-1)]
    public void SerializeDeserialize_Int(int value)
    {
        var serializer = new ReflectionBinarySerializer<int>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(int))(value, stream);
        stream.Position = 0;
        var deserializedValue = (int)serializer.DeserializerFor(typeof(int))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(42u)]
    [InlineData(uint.MinValue)]
    [InlineData(uint.MaxValue)]
    public void SerializeDeserialize_UInt(uint value)
    {
        var serializer = new ReflectionBinarySerializer<uint>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(uint))(value, stream);
        stream.Position = 0;
        var deserializedValue = (uint)serializer.DeserializerFor(typeof(uint))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(42L)]
    [InlineData(long.MinValue)]
    [InlineData(long.MaxValue)]
    [InlineData(-1L)]
    public void SerializeDeserialize_Long(long value)
    {
        var serializer = new ReflectionBinarySerializer<long>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(long))(value, stream);
        stream.Position = 0;
        var deserializedValue = (long)serializer.DeserializerFor(typeof(long))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(42uL)]
    [InlineData(ulong.MinValue)]
    [InlineData(ulong.MaxValue)]
    public void SerializeDeserialize_ULong(ulong value)
    {
        var serializer = new ReflectionBinarySerializer<ulong>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(ulong))(value, stream);
        stream.Position = 0;
        var deserializedValue = (ulong)serializer.DeserializerFor(typeof(ulong))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData((short)42)]
    [InlineData(short.MinValue)]
    [InlineData(short.MaxValue)]
    [InlineData((short)-1)]
    public void SerializeDeserialize_Short(short value)
    {
        var serializer = new ReflectionBinarySerializer<short>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(short))(value, stream);
        stream.Position = 0;
        var deserializedValue = (short)serializer.DeserializerFor(typeof(short))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData((ushort)42)]
    [InlineData(ushort.MinValue)]
    [InlineData(ushort.MaxValue)]
    public void SerializeDeserialize_UShort(ushort value)
    {
        var serializer = new ReflectionBinarySerializer<ushort>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(ushort))(value, stream);
        stream.Position = 0;
        var deserializedValue = (ushort)serializer.DeserializerFor(typeof(ushort))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData((byte)42)]
    [InlineData(byte.MinValue)]
    [InlineData(byte.MaxValue)]
    public void SerializeDeserialize_Byte(byte value)
    {
        var serializer = new ReflectionBinarySerializer<byte>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(byte))(value, stream);
        stream.Position = 0;
        var deserializedValue = (byte)serializer.DeserializerFor(typeof(byte))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData((sbyte)42)]
    [InlineData(sbyte.MinValue)]
    [InlineData(sbyte.MaxValue)]
    public void SerializeDeserialize_SByte(sbyte value)
    {
        var serializer = new ReflectionBinarySerializer<sbyte>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(sbyte))(value, stream);
        stream.Position = 0;
        var deserializedValue = (sbyte)serializer.DeserializerFor(typeof(sbyte))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void SerializeDeserialize_Bool(bool value)
    {
        var serializer = new ReflectionBinarySerializer<bool>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(bool))(value, stream);
        stream.Position = 0;
        var deserializedValue = (bool)serializer.DeserializerFor(typeof(bool))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData('A')]
    [InlineData('\0')]
    [InlineData(char.MaxValue)]
    public void SerializeDeserialize_Char(char value)
    {
        var serializer = new ReflectionBinarySerializer<char>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(char))(value, stream);
        stream.Position = 0;
        var deserializedValue = (char)serializer.DeserializerFor(typeof(char))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData("Hello, World!")]
    [InlineData("")]
    [InlineData(" ")]
    public void SerializeDeserialize_String(string value)
    {
        var serializer = new ReflectionBinarySerializer<string>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(string))(value, stream);
        stream.Position = 0;
        var deserializedValue = (string)serializer.DeserializerFor(typeof(string))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(3.14f)]
    [InlineData(float.MinValue)]
    [InlineData(float.MaxValue)]
    [InlineData(-3.14f)]
    public void SerializeDeserialize_Float(float value)
    {
        var serializer = new ReflectionBinarySerializer<float>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(float))(value, stream);
        stream.Position = 0;
        var deserializedValue = (float)serializer.DeserializerFor(typeof(float))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(3.14)]
    [InlineData(double.MinValue)]
    [InlineData(double.MaxValue)]
    [InlineData(-3.14)]
    public void SerializeDeserialize_Double(double value)
    {
        var serializer = new ReflectionBinarySerializer<double>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(double))(value, stream);
        stream.Position = 0;
        var deserializedValue = (double)serializer.DeserializerFor(typeof(double))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-42)]
    public void SerializeDeserialize_Decimal(decimal value)
    {
        var serializer = new ReflectionBinarySerializer<decimal>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(decimal))(value, stream);
        stream.Position = 0;
        var deserializedValue = (decimal)serializer.DeserializerFor(typeof(decimal))(stream);
        Assert.Equal(value, deserializedValue);
    }
    
    [Fact]
    public void SerializeDeserialize_DecimalMinMax()
    {
        var serializer = new ReflectionBinarySerializer<decimal>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(decimal))(decimal.MinValue, stream);
        stream.Position = 0;
        var deserializedValue = (decimal)serializer.DeserializerFor(typeof(decimal))(stream);
        Assert.Equal(decimal.MinValue, deserializedValue);

        stream = new MemoryStream();
        serializer.SerializerFor(typeof(decimal))(decimal.MaxValue, stream);
        stream.Position = 0;
        deserializedValue = (decimal)serializer.DeserializerFor(typeof(decimal))(stream);
        Assert.Equal(decimal.MaxValue, deserializedValue);
    }

    [Theory]
    [InlineData("2023-10-04T12:34:56.7890123Z")]
    [InlineData("0001-01-01T00:00:00.0000000Z")] // DateTime.MinValue
    [InlineData("4000-12-31T23:59:59.9999999Z")] // DateTime.MaxValue
    public void SerializeDeserialize_DateTime(DateTime value)
    {
        var serializer = new ReflectionBinarySerializer<DateTime>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(DateTime))(value, stream);
        stream.Position = 0;
        var deserializedValue = (DateTime)serializer.DeserializerFor(typeof(DateTime))(stream);
        Assert.Equal(value, deserializedValue);
    }

    [Theory]
    [InlineData(122.5d)]
    [InlineData(-122.5d)]
    public void SerializeDeserialize_TimeSpan(double seconds)
    {
        var value = TimeSpan.FromSeconds(seconds);
        var serializer = new ReflectionBinarySerializer<TimeSpan>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(TimeSpan))(value, stream);
        stream.Position = 0;
        var deserializedValue = (TimeSpan)serializer.DeserializerFor(typeof(TimeSpan))(stream);
        Assert.Equal(value, deserializedValue);
    }
}