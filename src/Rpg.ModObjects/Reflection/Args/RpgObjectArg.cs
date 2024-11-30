using System.Reflection;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Reflection.Args
{
    public class RpgObjectArg : RpgArg
    {
        [JsonConstructor] private RpgObjectArg() { }

        public RpgObjectArg(ParameterInfo parameterInfo)
            : base(parameterInfo)
        { }

        public override RpgArg Clone()
            => new RpgObjectArg 
            { 
                Name = Name, 
                Type = Type,
                IsNullable = IsNullable,
                Value = Value,
                Groups = Groups
            };

        public override void SetValue(object? value, RpgGraph? graph = null)
            => Value = Convert(value, graph);

        public override void FillValue(object? value, RpgGraph? graph = null)
            => Value ??= Convert(value, graph);

        private string? Convert(object? value, RpgGraph? graph)
        {
            if (value == null) return null;
            if (value is RpgObject obj && obj.Archetypes.Contains(Type))
                return obj.Id;
            //{
            //    var conversionType = RpgTypeScan.ForTypeByName(Type);
            //    return System.Convert.ChangeType(value, conversionType) as RpgObject;
            //}
            if (value is string id)
            {
                return graph != null
                    ? graph.GetObject(id)?.Id
                    : id;
            }
            throw new ArgumentException($"value not of type {Type}");
        }
    }
}
