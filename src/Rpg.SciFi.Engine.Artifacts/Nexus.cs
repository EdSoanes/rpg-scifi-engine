using Rpg.SciFi.Engine.Artifacts.Attributes;
using Rpg.SciFi.Engine.Artifacts.Turns;
using System.Collections;
using System.Reflection;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class Nexus
    {
        public static object? Context { get; set; }

        public static string[] GetActions(object context, string? basePath = null)
        {
            var res = new List<string>();

            var type = context.GetType();
            var actionMethods = type.GetMethods()
                .Where(x => x.GetCustomAttributes<AbilityAttribute>(true).Any() && x.ReturnType == typeof(TurnAction));

            foreach (var actionMethod in actionMethods)
            {
                var action = $"{type.Name}.{actionMethod.Name}()";
                res.Add(action);
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                var path = !string.IsNullOrEmpty(basePath)
                    ? $"{basePath}.{propertyInfo.Name}"
                    : $"{type.Name}.{propertyInfo.Name}";

                var val = propertyInfo.GetValue(context);
                var obj = GetPropertyObject(val);

                if (obj != null)
                {
                    var artifactId = ArtifactObjectId(obj);
                    if (artifactId != null)
                        path = $"{type.Name}[{artifactId.Value}].{propertyInfo.Name}";

                    var modifiableId = ModifiableObjectId(obj);
                    if (modifiableId != null)
                        res.Add(path);

                    var actions = GetActions(obj, path);
                    res.AddRange(actions);
                }
            }

            return res.ToArray();
        }

        private static object? GetPropertyObject(object? obj)
        {
            if (obj == null || obj is string || obj.GetType().IsPrimitive)
                return null;

            if (obj is IEnumerable)
                return null;

            return obj;
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
