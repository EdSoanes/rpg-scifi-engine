using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgTypes
{
    public class Int32ArgType : IRpgArgType
    {
        public string TypeName 
            => nameof(Int32);

        public string QualifiedTypeName 
            => typeof(Int32).AssemblyQualifiedName!;

        public bool IsNullable { get; set; } = true;

        public bool IsArgTypeFor(ParameterInfo parameterInfo)
            => parameterInfo.ParameterType == typeof(Int32) || parameterInfo.ParameterType == typeof(Int32?);

        public IRpgArgType Clone(Type? type = null)
        {
            var clone = Activator.CreateInstance<Int32ArgType>();
            clone.IsNullable = type != null ? type == typeof(Int32?) : false;
            return clone;
        }

        public bool IsValid(object? value)
            => int.TryParse(value?.ToString(), out int _);

        public string? ToArgString(object? value)
            => int.TryParse(value?.ToString(), out int result)
                ? result.ToString()
                : null;

        public object? ToArgObject(string? value) 
            => int.TryParse(value, out int result)
                ? (object?)result 
                : null;
    }
}
