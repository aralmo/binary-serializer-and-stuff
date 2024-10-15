using System;
using System.Collections.Generic;
using System.IO;
using BenchmarkDotNet;
using BenchmarkDotNet.Attributes;

namespace Benchmarks
{
    [MemoryDiagnoser]
    public class Benchmarks
    {
        [Benchmark]
        public void JsonSerialization()
        {
            var complexObject = CreateComplexObject();
            for (int i = 0; i < 1000; i++)
            {
                var jsonSerializer = new System.Text.Json.JsonSerializerOptions();
                var stream = new MemoryStream();
                System.Text.Json.JsonSerializer.Serialize(stream, complexObject, jsonSerializer);
                stream.Position = 0;
                var deserializedObject = System.Text.Json.JsonSerializer.Deserialize<ComplexTestClass>(stream, jsonSerializer);
            }
        }
        [Benchmark]
        public void BinarySerialization()
        {
            var complexObject = CreateComplexObject();
            for (int i = 0; i < 1000; i++)
            {
                var binarySerializer = new BinarySerializer<ComplexTestClass>();
                var stream = new MemoryStream();
                binarySerializer.SerializerFor(typeof(ComplexTestClass))(complexObject, stream);
                stream.Position = 0;
                var deserializedObject = (ComplexTestClass)binarySerializer.DeserializerFor(typeof(ComplexTestClass))(stream);
            }
        }

        private ComplexTestClass CreateComplexObject()
        {
            return new ComplexTestClass
            {
                Id = 42,
                Name = "Complex Test",
                NestedClass = new SimpleTestClass { Id = 100, Name = "Nested Class" },
                ArrayProperty = new int[] { 1, 2, 3, 4, 5 },
                ListProperty = new List<SimpleTestClass>
                {
                    new SimpleTestClass { Id = 1, Name = "List Item 1" },
                    new SimpleTestClass { Id = 2, Name = "List Item 2" }
                },
                DictionaryProperty = new Dictionary<string, string>
                {
                    { "Key1", "Value1" },
                    { "Key2", "Value2" }
                }
            };
        }

        public class ComplexTestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public SimpleTestClass NestedClass { get; set; }
            public int[] ArrayProperty { get; set; }
            public List<SimpleTestClass> ListProperty { get; set; }
            public Dictionary<string, string> DictionaryProperty { get; set; }
        }

        public class SimpleTestClass
        {
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}