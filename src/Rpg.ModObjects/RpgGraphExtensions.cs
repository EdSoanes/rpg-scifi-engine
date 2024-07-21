using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props;
using System.Collections;
using System.Reflection;

namespace Rpg.ModObjects
{
    public static class RpgGraphExtensions
    {

        private static Type[] ExcludedPropertyTypes = new Type[]
        {
            typeof(RpgGraph),
            typeof(ModSet),
            typeof(Mod),
            typeof(string),
            typeof(DateTime),
            typeof(Guid)
        };

        internal static IEnumerable<RpgObject> Traverse(this object obj, bool bottomUp = false)
        {
            var entity = obj as RpgObject;
            if (entity != null && !bottomUp)
                yield return entity;

            if (!IsExcludedType(obj.GetType()))
            {
                var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var items = obj.GetPropertyObjects(propertyInfo, out var isEnumerable);
                    foreach (var item in items)
                    {
                        var childEntities = item.Traverse(bottomUp);
                        foreach (var childEntity in childEntities)
                            yield return childEntity;
                    }
                }
            }

            if (entity != null && bottomUp)
                yield return entity;
        }

        internal static IEnumerable<T> Traverse<T, TStop>(this object obj)
            where T : RpgObject
            where TStop : RpgObject
        {
            var component = obj as T;
            if (component != null)
                yield return component;

            if (!IsExcludedType(obj.GetType()))
            {
                var propertyInfos = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var items = obj.GetPropertyObjects(propertyInfo, out var isEnumerable);
                    foreach (var item in items.Where(x => x is not TStop))
                    {
                        var childComponents = item.Traverse<T, TStop>();
                        foreach (var childComponent in childComponents)
                            yield return childComponent;
                    }
                }
            }

            if (component != null)
                yield return component;
        }

        internal static void Merge(this List<PropRef> target, PropRef propRef)
        {
            if (!target.Any(x => x == propRef))
                target.Add(propRef);
        }

        internal static void Merge(this List<PropRef> target, IEnumerable<PropRef> source)
        {
            foreach (var a in source)
                target.Merge(a);
        }

        internal static (RpgObject?, string?) FromPath(this RpgObject? rootEntity, string propPath)
        {
            if (rootEntity == null)
                return (null, null);

            var parts = propPath.Split('.');
            var path = string.Join(".", parts.Take(parts.Length - 1));
            var prop = parts.Last();

            return !string.IsNullOrEmpty(path)
                ? (rootEntity.PropertyValue<RpgObject>(path), prop)
                : (rootEntity, prop);
        }

        internal static string[] PathTo(this object obj, object? descendent)
        {
            var propStack = new Stack<string>();

            if (descendent != null)
            {
                var res = PathTo(propStack, obj, descendent);
                if (!res)
                    return new string[0];
            }

            return propStack.ToArray();
        }

        private static bool PathTo(Stack<string> propStack, object obj, object descendent)
        {
            if (obj == descendent)
                return true;

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                propStack.Push(propertyInfo.Name);

                var children = obj.GetPropertyObjects(propertyInfo, out var isEnumerable)?.ToArray() ?? new object[0];
                if (isEnumerable)
                {
                    propStack.Pop();
                    return false;
                }

                if (children.Count() == 1 && PathTo(propStack, children.First(), descendent))
                    return true;

                propStack.Pop();
            }

            return false;
        }

        private static IEnumerable<object> GetPropertyObjects(this object context, PropertyInfo propertyInfo, out bool isEnumerable)
        {
            isEnumerable = false;

            if (propertyInfo.GetMethod?.Name == "get_Item" || IsExcludedType(propertyInfo.PropertyType) || propertyInfo.PropertyType.IsPrimitive)
                return Enumerable.Empty<object>();

            var obj = propertyInfo.GetValue(context, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, null, null);

            if (obj == null)
                return Enumerable.Empty<object>();

            var items = GetPropObjects(obj!, out isEnumerable);
            return items;
        }

        private static List<object> GetPropObjects(object? obj, out bool isEnumerable)
        {
            isEnumerable = false;

            var res = new List<object>();
            var items = new List<object?>();
            if (obj is IDictionary)
            {
                items = (obj as IDictionary)!.Values.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj is IEnumerable)
            {
                items = (obj as IEnumerable)!.Cast<object?>().ToList();
                isEnumerable = true;
            }
            else if (obj != null)
                res.Add(obj);

            foreach (var item in items.Where(x => x != null))
                res.AddRange(GetPropObjects(item, out var _));

            return res;
        }

        private static bool IsExcludedType(Type type)
            => !type.IsPrimitive && ExcludedPropertyTypes.Any(type.IsAssignableTo);
    }
}
