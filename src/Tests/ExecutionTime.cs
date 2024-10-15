using System.Diagnostics;
using System.Text.Json;
using FluentAssertions;
namespace Serialization;
public class ExecutionTimeTests
{
    [Fact]
    public void CompareSerializationExecutionTime()
    {
        // Setup test data
        var testObject = new GenericContainer
        {
            IntList = new List<int> { 1, 2, 3, 4, 5 },
            StringList = new List<string> { "Hello", "World", "Test", "Data" },
            ComplexList = new List<ComplexType>
            {
                new ComplexType { Id = 1, Name = "First" },
                new ComplexType { Id = 2, Name = "Second" }
            }
        };

        // Binary serialization
        var binarySerializer = new BinarySerializer<GenericContainer>();
        var binaryStream = new MemoryStream();
        var binaryStopwatch = Stopwatch.StartNew();
        binarySerializer.SerializerFor(typeof(GenericContainer))(testObject, binaryStream);
        binaryStream.Position = 0;
        var binaryDeserialized = (GenericContainer)binarySerializer.DeserializerFor(typeof(GenericContainer))(binaryStream);
        binaryStopwatch.Stop();

        // System.Text.Json serialization
        var jsonStopwatch = Stopwatch.StartNew();
        var jsonString = JsonSerializer.Serialize(testObject);
        var jsonDeserialized = JsonSerializer.Deserialize<GenericContainer>(jsonString);
        jsonStopwatch.Stop();

        // Assert execution time
        binaryStopwatch.Elapsed.Should().BeLessThan(jsonStopwatch.Elapsed, "Binary serialization should be faster than System.Text.Json serialization.");
    }

    public class GenericContainer
    {
        public List<int> IntList { get; set; }
        public List<string> StringList { get; set; }
        public List<ComplexType> ComplexList { get; set; }
    }

    public class ComplexType
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
