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
    public class DiceArg : RpgArg
    {
        [JsonConstructor] private DiceArg() { }

        public DiceArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override bool IsValid(object? value)
            => Dice.TryParse(value?.ToString(), out Dice _);

        public override string? ToArgString(object? value)
            => Dice.TryParse(value?.ToString(), out Dice result)
                ? result.ToString()
                : null;

        public override object? ToArgObject(string? value)
            => Dice.TryParse(value, out Dice result)
                ? (object?)result
                : null;
    }
}
