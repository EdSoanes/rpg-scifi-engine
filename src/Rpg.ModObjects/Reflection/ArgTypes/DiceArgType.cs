using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Reflection.ArgTypes
{
    public class DiceArgType : IRpgArgType
    {
        public string TypeName 
            => nameof(Dice);

        public string QualifiedTypeName 
            => typeof(Dice).AssemblyQualifiedName!;

        public bool IsNullable { get; set; } = true;

        public bool IsArgTypeFor(ParameterInfo parameterInfo)
            =>  parameterInfo.ParameterType == typeof(Dice) || parameterInfo.ParameterType == typeof(Dice?);

        public IRpgArgType Clone(Type? type = null)
        {
            var clone = Activator.CreateInstance<DiceArgType>();
            clone.IsNullable = type == typeof(Dice?);
            return clone;
        }

        public bool IsValid(object? value)
            => Dice.TryParse(value?.ToString(), out Dice _);

        public string? ToArgString(object? value)
            => Dice.TryParse(value?.ToString(), out Dice result)
                ? result.ToString()
                : null;

        public object? ToArgObject(string? value) 
            => Dice.TryParse(value, out Dice result)
                ? (object?)result 
                : null;
    }
}
