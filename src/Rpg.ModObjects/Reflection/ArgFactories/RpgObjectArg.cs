using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgFactories
{
    public class RpgObjectArg : RpgArg
    {
        public RpgObjectArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override bool IsValid(object? value)
            => Dice.TryParse(value?.ToString(), out Dice _);

        public override string? ToArgString(object? value)
            => (value as RpgObject)?.Id;

        public override object? ToArgObject(string? value)
            => value;
    }
}
