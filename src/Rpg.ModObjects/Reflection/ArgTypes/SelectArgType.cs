using System.Reflection;

namespace Rpg.ModObjects.Reflection.ArgTypes
{
    public abstract class SelectArgType : IRpgArgType
    {
        public string TypeName
            => nameof(Int32);

        public string QualifiedTypeName
            => typeof(Int32).AssemblyQualifiedName!;

        public bool IsNullable { get; set; } = true;

        public string[] Options { get; set; } 
            = Array.Empty<string>();

        public bool IsArgTypeFor(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType == typeof(Int32);

        public IRpgArgType Clone(Type? type = null)
        {
            var clone = Activator.CreateInstance<SelectArgType>();
            clone.IsNullable = IsNullable;
            clone.Options = Options.ToArray();
            return clone;
        }

        public bool IsValid(object? value)
        {
            if (value == null)
                return false;

            var val = Array.IndexOf(Options, value.ToString());
            return val >= 0;
        }

        public string? ToArgString(object? value)
        {
            var val = Array.IndexOf(Options, value?.ToString() ?? string.Empty);
            return val >= 0 ? val.ToString() : null;
        }

        public object? ToArgObject(string? value)
        {
            var val = Array.IndexOf(Options, value ?? string.Empty);
            return val >= 0 ? val : null;
        }
    }
}
