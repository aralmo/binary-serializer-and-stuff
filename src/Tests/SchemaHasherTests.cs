using System;
using System.Reflection;
using System.Reflection.Emit;
using Xunit;

namespace SerializationTests;
public class SchemaHasherTests
{
    [Fact]
    public void ReturnsConsistentHash()
    {
        // Arrange
        var hasher = new Serialization.SchemaHasher();
        var settings = new SerializationSettings();
        var dynamicType = CreateDynamicType(new[]
        {
            ("SomeProperty", typeof(string))
        });

        // Act
        var hash1 = hasher.HashSchema(dynamicType, settings);
        var hash2 = hasher.HashSchema(dynamicType, settings);

        // Assert
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void HashChangesWhenArrayTypeChanges()
    {
        // Arrange
        var hasher = new Serialization.SchemaHasher();
        var settings = new SerializationSettings();
        var dynamicTypeOriginal = CreateDynamicType(new[]
        {
            ("StringProperty", typeof(string)),
            ("IntProperty", typeof(int)),
            ("ListProperty", typeof(List<decimal>)),
            ("DictionaryProperty", typeof(Dictionary<string, Guid>)),
            ("ArrayProperty", typeof(byte[]))
        });

        var dynamicTypeModified = CreateDynamicType(new[]
        {
            ("StringProperty", typeof(string)),
            ("IntProperty", typeof(int)),
            ("ListProperty", typeof(List<decimal>)),
            ("DictionaryProperty", typeof(Dictionary<string, Guid>)),
            ("ArrayProperty", typeof(int[])) // change
        });

        // Act
        var hash1 = hasher.HashSchema(dynamicTypeOriginal, settings);
        var hash2 = hasher.HashSchema(dynamicTypeModified, settings);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashChangesWhenGenericTypeChanges()
    {
        // Arrange
        var hasher = new Serialization.SchemaHasher();
        var settings = new SerializationSettings();
        var dynamicTypeOriginal = CreateDynamicType(new[]
        {
            ("StringProperty", typeof(string)),
            ("IntProperty", typeof(int)),
            ("ListProperty", typeof(List<decimal>)),
            ("DictionaryProperty", typeof(Dictionary<string, Guid>)),
            ("ArrayProperty", typeof(byte[]))
        });

        var dynamicTypeModified = CreateDynamicType(new[]
        {
            ("StringProperty", typeof(string)),
            ("IntProperty", typeof(int)),
            ("ListProperty", typeof(List<DateTime>)), // change
            ("DictionaryProperty", typeof(Dictionary<string, Guid>)),
            ("ArrayProperty", typeof(byte[]))
        });

        // Act
        var hash1 = hasher.HashSchema(dynamicTypeOriginal, settings);
        var hash2 = hasher.HashSchema(dynamicTypeModified, settings);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    public void HashChangesWhenRelatedTypePropertyChanges()
    {
        // Arrange
        var hasher = new Serialization.SchemaHasher();
        var settings = new SerializationSettings();

        var relatedTypeOriginal = CreateDynamicType(new[]
        {
            ("NestedStringProperty", typeof(string))
        });
        var relatedTypeModified = CreateDynamicType(new[]
        {
            ("NestedStringProperty", typeof(int)) // change
        });

        var dynamicTypeOriginal = CreateDynamicType(new[]
        {
            ("RelatedTypeProperty", relatedTypeOriginal)
        });

        var dynamicTypeModified = CreateDynamicType(new[]
        {
            ("RelatedTypeProperty", relatedTypeModified)
        });

        // Act
        var hash1 = hasher.HashSchema(dynamicTypeOriginal, settings);
        var hash2 = hasher.HashSchema(dynamicTypeModified, settings);

        // Assert
        Assert.NotEqual(hash1, hash2);
    }


    private Type CreateDynamicType((string Name, Type Type)[] properties)
    {
        var assemblyName = new AssemblyName("DynamicAssembly");
        var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule");
        var typeBuilder = moduleBuilder.DefineType("DynamicType", TypeAttributes.Public);

        foreach (var (propertyName, propertyType) in properties)
        {
            var fieldName = $"_{propertyName.ToLower()}";
            var fieldBuilder = typeBuilder.DefineField(fieldName, propertyType, FieldAttributes.Private);
            var propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMthdBldr = typeBuilder.DefineMethod($"get_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);
            var setPropMthdBldr = typeBuilder.DefineMethod($"set_{propertyName}", MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, null, new[] { propertyType });
            var setIl = setPropMthdBldr.GetILGenerator();
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);
            setIl.Emit(OpCodes.Ret);
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        return typeBuilder.CreateType();
    }
}
