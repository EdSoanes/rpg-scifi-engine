using System;
using System.Linq.Expressions;
using System.Reflection;
using NanoidDotNet;
using Rpg.Experimental.Graph;

namespace Rpg.Experimental
{
    public static class RpgObjectExtensions
    {
        private const string Alphabet = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const int Size = 10;

        public static string NewId(this object obj)
            => $"{obj.GetType().Name}[{Nanoid.Generate(Alphabet, Size)}]";

        internal static RpgPropertyRef? PropertyRef(this object? obj, string path)
            => obj.GetPropertyRefValue(path).PropertyRef;

        internal static RpgPropertyRef? PropertyRef<T, TResult>(this T obj, Expression<Func<T, TResult>> expression)
            where T : RpgObject
                => obj.GetPropertyRefValue(expression)?.PropertyRef;

        public static void SetPropertyValue<T>(this RpgObject? obj, string path, T? value)
        {
            var (propObj, prop) = obj.GetObjectByPath(path);
            if (propObj != null && prop != null)
            {
                var propInfo = propObj.GetType().GetProperty(prop);
                var setMethod = propInfo?.GetSetMethod(true);
                if (propInfo != null && setMethod != null && RpgTypeUtilities.PropertyOfType(propInfo, typeof(T)))
                    setMethod.Invoke(propObj, [value]);
            }
        }

        internal static T? Value<T>(this RpgObject? obj, string path)
        {
            var (propObj, prop) = obj.GetObjectByPath(path);
            if (propObj != null && prop != null)
            {
                var value = propObj.GetType().GetProperty(prop)?.GetValue(propObj);
                if (value is T)
                    return (T)value;
            }

            return default;
        }

        internal static RpgPropertyRefValue PropertyRefValue(this object? obj, string path)
            => obj.GetPropertyRefValue(path);

        internal static RpgPropertyRefValue PropertyRefValue<T>(this object? obj, string path)
            => obj.GetPropertyRefValue(path, typeof(T));

        internal static RpgPropertyRefValue PropertyRefValue<T, TResult>(this T obj, Expression<Func<T, TResult>> expression)
            where T : RpgObject
            => obj.GetPropertyRefValue(expression);
        
        private static RpgPropertyRefValue GetPropertyRefValue<T, TResult>(this T obj, Expression<Func<T, TResult>> expression)
            where T : RpgObject
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
                throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

            var pathSegs = new List<string>();
            pathSegs.Add(memberExpression.Member.Name);
            while (memberExpression != null)
            {
                memberExpression = memberExpression.Expression as MemberExpression;
                if (memberExpression != null)
                    pathSegs.Add(memberExpression.Member.Name);
            }

            pathSegs.Reverse();
            var path = string.Join(".", pathSegs);

            return obj.PropertyRefValue(path)!;
        }

        private static RpgPropertyRefValue GetPropertyRefValue(this object? obj, string path, Type? valueType = null)
        {
            var (propObj, prop) = obj.GetObjectByPath(path);
            if (propObj != null && prop != null)
            {
                var propInfo = propObj.GetType().GetProperty(prop);
                if (propInfo != null && (valueType == null || IsPropertyTypeMatch(propInfo.PropertyType, valueType)))
                    return new RpgPropertyRefValue(new RpgPropertyRef(propObj.Id, propInfo.Name), propInfo.GetValue(propObj));
            }

            return new RpgPropertyRefValue(null, null);
        }

        internal static (RpgObject?, string?) GetObjectByPath(this object? obj, string path)
        {
            if (obj == null || string.IsNullOrEmpty(path))
                return (null, null);

            var parts = path.Split('.');
            PropertyInfo? propInfo = null;

            for (int i = 0; i < parts.Length; i++)
            {
                var part = parts[i];

                propInfo = obj.GetType().GetProperty(part, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (propInfo == null)
                    return (null, null);

                if (i < parts.Length - 1)
                {
                    obj = propInfo.GetValue(obj, null);
                    if (obj == null)
                        return (null, null);
                }
            }

            return (
                obj as RpgObject,
                obj is RpgObject ? parts.Last() : null
            );
        }

        private static bool IsPropertyTypeMatch(Type propertyType, Type valueType)
            => valueType.IsAssignableTo(propertyType) || (Nullable.GetUnderlyingType(propertyType)?.IsAssignableFrom(valueType) ?? false);
    }
}

