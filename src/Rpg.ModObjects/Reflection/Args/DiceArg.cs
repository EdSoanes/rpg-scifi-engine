using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Reflection;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DiceArg : RpgArg
    {
        [JsonConstructor] private DiceArg() { }

        public DiceArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override RpgArg Clone()
            => new DiceArg
            {
                Name = Name,
                Type = Type,
                IsNullable = IsNullable,
                Value = Value,
                Groups = Groups
            };

        public override void SetValue(object? value, RpgGraph? graph = null)
            => Value = Convert(value);

        public override void FillValue(object? value, RpgGraph? graph = null)
            => Value ??= Convert(value);

        private Dice? Convert(object? value)
        {
            if (value == null) return null;
            if (value is Dice dice) return dice;
            if (Dice.TryParse(value?.ToString(), out Dice result)) return result;
            if (int.TryParse(value?.ToString(), out int val)) return new Dice(val);

            throw new ArgumentException($"value {value} invalid");
        }
    }
}
