using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Cms.Json
{
    public class JProp<T>
    {
        public string Expr { get; private set; }
        public string Name { get; set; }
        public string[] Path { get; set; } = new string[0];

        public JProp(string propExpr)
        {
            Expr = propExpr;

            var segments = ParseSegments(propExpr);
            if (!segments.Any())
                throw new JOpException($"Invalid property expression {propExpr}");

            var nameParts = segments.Last().Split('=');
            if (nameParts.Length is < 1 or > 2)
                throw new JOpException($"Invalid property expression {propExpr}");

            Name = nameParts.First();

            //Not using this here, just keeping the code...
            var defaultValue = GetDefaultValue(nameParts.Length == 2
                ? nameParts[1]
                : null);

            if (segments.Length > 1)
                Path = segments.Take(segments.Length - 1).ToArray();
        }

        private string[] ParseSegments(string propExpr)
            => propExpr?
                .Split('.')
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray() ?? new string[0];

        private T? GetDefaultValue(string? val)
        {
            if (string.IsNullOrEmpty(val))
                return default;

            if (typeof(T) == typeof(string))
                return (T)(object)val;

            if (typeof(T) == typeof(int) && int.TryParse(val, out var i))
                return (T)(object)i;

            return default;
        }
    }

}
