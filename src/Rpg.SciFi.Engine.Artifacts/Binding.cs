using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public static class Binding
    {
        public static string? Evaluate(object? context, string bindingExpr)
        {
            if (context == null)
                throw new Exception("Empty context");

            var parts = bindingExpr.Split('.');
            if (parts.Length == 0)
                throw new Exception("Empty binding expression");

            return GetValue(context, parts)?.ToString();
        }

        private static object? GetValue(object? context, string[] path)
        {
            if (context == null)
                throw new Exception("Empty context");

            if (path.Length == 0)
                throw new Exception("Empty Path");

            var propInfo = context?.GetType().GetProperty(path[0]);
            if (propInfo == null)
                throw new Exception("Invalid Path");

            var propVal = propInfo.GetValue(context);
            if (path.Length > 1)
            {
                if (propVal == null)
                    throw new Exception("Invalid Path, null value");

                return GetValue(propVal, path.Skip(1).ToArray());
            }

            return propVal;
        }
    }
}
