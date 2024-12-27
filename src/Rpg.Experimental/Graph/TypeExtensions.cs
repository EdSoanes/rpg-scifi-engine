using System;
using System.Reflection;

namespace Rpg.Experimental.Graph
{
    internal static class TypeExtensions
    {
        internal static bool PropertyOfType(this PropertyInfo? propertyInfo, Type valueType)
            => propertyInfo != null && propertyInfo.PropertyType.PropertyOfType(valueType);

        internal static bool PropertyOfType(this Type propertyType, Type valueType)
            => propertyType.IsAssignableTo(valueType) || propertyType.PropertyOfNullableType(valueType);

        internal static bool PropertyOfNullableType(this Type propertyType, Type valueType)
        {
            var underlyingType = Nullable.GetUnderlyingType(propertyType);
            return underlyingType?.IsAssignableTo(propertyType) ?? false;
        }
    }
}
