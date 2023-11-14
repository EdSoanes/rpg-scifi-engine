using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class Nexus
    {
        public static object? Context { get; set; }

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
