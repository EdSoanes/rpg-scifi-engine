using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection
{
    public interface IRpgArgType
    {
        string TypeName { get; }
        string QualifiedTypeName { get; }
        bool IsNullable { get; set; }

        bool IsArgTypeFor(ParameterInfo parameterInfo);
        IRpgArgType Clone(Type? type = null);
        bool IsValid(object? value);
        string? ToArgString(object? value);
        object? ToArgObject(string? value);
    }
}
