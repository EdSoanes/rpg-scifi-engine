using System.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Reflection.Args
{
    public class DefaultArg : RpgArg
    {
        [JsonConstructor] private DefaultArg() { }

        public DefaultArg(ParameterInfo parameterInfo) 
            : base(parameterInfo)
        { }

        public override RpgArg Clone()
            => new DefaultArg 
            { 
                Name = Name, 
                Type = Type,
                IsNullable = IsNullable,
                Value = Value, 
                Groups = Groups 
            };

        public override void SetValue(object? value, RpgGraph? graph = null)
            => Value = value;

        public override void FillValue(object? value, RpgGraph? graph = null)
            => Value ??= value;
    }
}
