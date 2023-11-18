using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Collections;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class Nexus
    {
        public static List<string> Actions = new List<string>();
        public static List<string> Props = new List<string>();
        public static ConcurrentDictionary<Guid, object> Contexts = new ConcurrentDictionary<Guid, object>();

        public static object? Context { get; set; }

        public static void BuildActionableLists()
        {
            Actions.Clear();
            Props.Clear();

            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Union(Assembly.GetCallingAssembly().GetTypes());

            foreach (var type in types)
            {
                if (type.IsAssignableTo(typeof(Artifact)))
                {
                    var actionMethods = type
                        .GetMethods()
                        .Where(x => x.GetCustomAttributes<AbilityAttribute>(true).Any() && x.ReturnType == typeof(TurnAction));

                    foreach (var actionMethod in actionMethods)
                    {
                        var action = $"{nameof(Artifact)}[{type.Name}].{actionMethod.Name}()";
                        Actions.Add(action);
                    }
                }

                if (type.IsAssignableTo(typeof(Modifiable)))
                {
                    foreach (var propertyInfo in type.GetProperties())
                    {
                        var pt = propertyInfo.PropertyType;
                        if ((pt.IsPrimitive || pt == typeof(string) || pt == typeof(Dice)) && propertyInfo.SetMethod == null)
                            Props.Add($"{nameof(Modifiable)}[{type.Name}].{propertyInfo.Name}");
                    }
                }
            }
        }

        public static List<string> GetPropertyPaths(object context, string? basePath = null)
        {
            var props = new List<string>();

            foreach (var propertyInfo in context.GetType().GetProperties())
            {
                var pt = propertyInfo.PropertyType;
                var prop = basePath == null
                    ? propertyInfo.Name
                    : $"{basePath}.{propertyInfo.Name}";

                if (pt.IsAssignableTo(typeof(IEnumerable)))
                    continue;
                
                if ((pt.IsPrimitive || pt == typeof(string) || pt == typeof(Dice)) && propertyInfo.SetMethod == null)
                {
                    props.Add(prop);
                    continue;
                }

                if (!pt.IsPrimitive && pt != typeof(string) && pt != typeof(Dice))
                {
                    var val = propertyInfo.GetValue(context);
                    if (val != null)
                    {
                        var subProps = GetPropertyPaths(val, prop);
                        props.AddRange(subProps);
                    }
                }
            }

            if (basePath == null)
                props = props.Distinct().OrderBy(x => x).ToList();

            return props;
        }

        private static IEnumerable<object> GetPropertyObjects(object? obj, out bool isEnumerable)
        {
            isEnumerable = false;

            if (obj == null || obj is string || obj.GetType().IsPrimitive)
                return Enumerable.Empty<object>();

            if (obj is IEnumerable)
            {
                isEnumerable = true;
                return (obj as IEnumerable)!.Cast<object>();
            }

            return new List<object> { obj };
        }

        private static bool IsCandidateObject(object? obj)
        {
            return obj != null && !(obj is string) && !obj.GetType().IsPrimitive;
        }

        public static bool IsCandidateType(Type type)
        {
            return !type.IsPrimitive && !type.IsEnum && type != typeof(string);
        }

        private static Guid? ModifiableObjectId(object? obj)
        {
            return obj != null
                ? (obj as Modifiable)?.Id
                : null;
        }

        private static Guid? ArtifactObjectId(object? obj)
        {
            return obj != null
                ? (obj as Artifact)?.Id
                : null;
        }

        public static T? EvaluateProperty<T>(string propExpr)
        {
            if (Context == null)
                throw new ArgumentException($"Context is null. Cannot evaluate property");

            if (string.IsNullOrEmpty(propExpr))
                throw new ArgumentException($"Empty propExpr. Cannot evaluate property");

            var parts = propExpr.Split('.');
            if (parts.Length == 0)
                throw new ArgumentException($"{propExpr} contains no valid path segments");

            return typeof(T) == typeof(string)
                ? (T?)(object?)GetPropertyValue(Context, parts)?.ToString()
                : (T?)GetPropertyValue(Context, parts);
        }


        private static object? GetPropertyValue(object? context, string[] path)
        {
            if (context == null)
                throw new ArgumentException($"Context is null. Cannot get property value");

            if (path.Length == 0)
                throw new Exception("Empty Path. Cannot get property value");

            var propInfo = context.GetType().GetProperty(path[0]);
            if (propInfo == null)
                throw new Exception($"{path[0]} does not exist on context {context.GetType().Name}");

            var propVal = propInfo.GetValue(context);
            if (path.Length > 1)
            {
                if (propVal == null)
                    throw new Exception($"{path[0]} is null on {context.GetType().Name}");

                return GetPropertyValue(propVal, path.Skip(1).ToArray());
            }

            return propVal;
        }
    }
}
