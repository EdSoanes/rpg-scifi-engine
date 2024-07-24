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
    public class DefaultArg : RpgArg
    {
        [JsonConstructor] private DefaultArg() { }

        public DefaultArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override bool IsValid(object? value)
            => true;

        public override string? ToArgString(object? value)
            => value?.ToString();

        public override object? ToArgObject(string? value)
            => null;
    }
}
