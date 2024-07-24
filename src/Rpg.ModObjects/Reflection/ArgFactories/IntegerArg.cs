using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public class IntegerArg : RpgArg
    {
        [JsonConstructor] private IntegerArg() { }

        public IntegerArg(ParameterInfo parameterInfo)
            : base(parameterInfo)
        { }

        public override bool IsValid(object? value)
            => int.TryParse(value?.ToString(), out int _);

        public override string? ToArgString(object? value)
            => int.TryParse(value?.ToString(), out int result)
                ? result.ToString()
                : null;

        public override object? ToArgObject(string? value)
            => int.TryParse(value, out int result)
                ? (object?)result
                : null;
    }
}
