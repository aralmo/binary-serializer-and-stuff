using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;

namespace SerializationTests.Serialization
{
    /// <summary>
    /// This class can generate a recursive schema 160bits sha1 hash for a class.
    /// This is so you can almost surely trust that a class schema hasn't changed.
    /// </summary>
    internal class SchemaHasher
    {
        public byte[] HashSchema(Type type, SerializationSettings settings)
        {
            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                var schemaData = GetSchemaData(type, settings, new HashSet<Type>());
                return sha1.ComputeHash(Encoding.UTF8.GetBytes(schemaData));
            }
        }

        private string GetSchemaData(Type type, SerializationSettings settings, HashSet<Type> visitedTypes)
        {
            if (visitedTypes.Contains(type))
                return string.Empty; // Prevent infinite recursion for cyclic dependencies

            visitedTypes.Add(type);

            var bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (settings.SerializePrivates) bindingFlags |= BindingFlags.NonPublic;

            var properties = type.GetProperties(bindingFlags);
            var sb = new StringBuilder();
            sb.Append(type.AssemblyQualifiedName).Append("#");
            foreach (var property in properties)
            {
                var propertyType = property.PropertyType;
                sb.Append(property.Name);
                sb.Append(":");
                sb.Append(GetFullTypeName(propertyType, settings));
                sb.Append(";");

                if (propertyType.IsClass && propertyType != typeof(string))
                {
                    sb.Append(GetSchemaData(propertyType, settings, visitedTypes));
                }
            }

            visitedTypes.Remove(type);

            return sb.ToString();
        }

        private string GetFullTypeName(Type type, SerializationSettings settings)
        {
            if (type.IsGenericType)
            {
                var genericArguments = type.GetGenericArguments();
                var genericArgumentsData = genericArguments.Select(t => GetSchemaData(t, settings, new HashSet<Type>())).ToArray();
                return $"{type.FullName}[{string.Join(",", genericArgumentsData)}]";
            }
            else
            {
                return type.FullName;
            }
        }
    }
}
