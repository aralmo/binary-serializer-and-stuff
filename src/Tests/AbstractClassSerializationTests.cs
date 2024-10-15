namespace Serialization;
public class AbstractTypeSerializationTests
{
    [Fact]
    public void SerializeDeserialize_AbstractClassWithArray()
    {
        var parent = new ParentClass
        {
            Children = new BaseClass[]
            {
                new ChildClass1 { Id = 1, Name = "Child 1", UniqueChild1Property = "Unique to Child 1" },
                new ChildClass2 { Id = 2, Name = "Child 2", UniqueChild2Property = 200 }
            }
        };

        var serializer = new BinarySerializer<ParentClass>();
        var stream = new MemoryStream();

        // Serialize
        serializer.SerializerFor(typeof(ParentClass))(parent, stream);

        // Reset stream position
        stream.Position = 0;

        // Deserialize
        var deserializedParent = (ParentClass)serializer.DeserializerFor(typeof(ParentClass))(stream);

        // Assertions
        Assert.NotNull(deserializedParent);
        Assert.NotNull(deserializedParent.Children);
        Assert.Equal(parent.Children.Length, deserializedParent.Children.Length);
        Assert.IsType<ChildClass1>(deserializedParent.Children[0]);
        Assert.IsType<ChildClass2>(deserializedParent.Children[1]);
        Assert.Equal(parent.Children[0].Id, deserializedParent.Children[0].Id);
        Assert.Equal(parent.Children[0].Name, deserializedParent.Children[0].Name);
        Assert.Equal(((ChildClass1)parent.Children[0]).UniqueChild1Property, ((ChildClass1)deserializedParent.Children[0]).UniqueChild1Property);
        Assert.Equal(parent.Children[1].Id, deserializedParent.Children[1].Id);
        Assert.Equal(parent.Children[1].Name, deserializedParent.Children[1].Name);
        Assert.Equal(((ChildClass2)parent.Children[1]).UniqueChild2Property, ((ChildClass2)deserializedParent.Children[1]).UniqueChild2Property);
    }

    public class ParentClass
    {
        public BaseClass[] Children { get; set; }
    }

    public abstract class BaseClass
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class ChildClass1 : BaseClass
    {
        public string UniqueChild1Property { get; set; }
    }

    public class ChildClass2 : BaseClass
    {
        public int UniqueChild2Property { get; set; }
    }
}