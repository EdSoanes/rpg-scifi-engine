using Rpg.ModObjects.Values;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rpg.ModObjects.Reflection.Args
{
    public class IntegerArg : RpgArg
    {
        [JsonConstructor] private IntegerArg() { }

        public IntegerArg(ParameterInfo parameterInfo)
            : base(parameterInfo)
        { }

        public override RpgArg Clone()
            => new IntegerArg
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

        private int? Convert(object? value)
        {
            if (value == null) return null;
            if (int.TryParse(value?.ToString(), out int i)) return i;
            if (Dice.TryParse(value?.ToString(), out var dice)) return dice.Roll();
            throw new ArgumentException($"value {value} invalid");
        }
    }
}
