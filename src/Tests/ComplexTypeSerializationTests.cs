using FluentAssertions;
using Xunit.Sdk;

namespace SerializationTests;
public class ComplexTypeSerializationTests
{
    [Fact]
    public void SerializeDeserialize_SimpleTestClass()
    {
        var testObject = new SimpleTestClass { Id = 42, Name = "Test Name" };
        var serializer = new BinarySerializer<SimpleTestClass>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(SimpleTestClass))(testObject, stream);
        stream.Position = 0;
        var deserializedObject = (SimpleTestClass)serializer.DeserializerFor(typeof(SimpleTestClass))(stream);
        Assert.Equal(testObject.Id, deserializedObject.Id);
        Assert.Equal(testObject.Name, deserializedObject.Name);
    }

    [Fact]
    public void SerializeDeserialize_ComplexTestClass()
    {
        var testObject = new ComplexTestClass
        {
            Id = 42,
            Name = "Complex Test",
            NestedClass = new SimpleTestClass { Id = 100, Name = "Nested Class" }
        };
        var serializer = new BinarySerializer<ComplexTestClass>();
        var stream = new MemoryStream();
        serializer.SerializerFor(typeof(ComplexTestClass))(testObject, stream);
        stream.Position = 0;
        var deserializedObject = (ComplexTestClass)serializer.DeserializerFor(typeof(ComplexTestClass))(stream);
        Assert.Equal(testObject.Id, deserializedObject.Id);
        Assert.Equal(testObject.Name, deserializedObject.Name);
        Assert.NotNull(deserializedObject.NestedClass);
        Assert.Equal(testObject.NestedClass.Id, deserializedObject.NestedClass.Id);
        Assert.Equal(testObject.NestedClass.Name, deserializedObject.NestedClass.Name);
    }

    public class SimpleTestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ComplexTestClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public SimpleTestClass NestedClass { get; set; }
    }
}