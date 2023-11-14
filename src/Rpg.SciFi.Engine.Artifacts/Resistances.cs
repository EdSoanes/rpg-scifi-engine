using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class BaseResistance
    {
        public BaseResistance() { }
        public BaseResistance(string name, string? description, int value)
        {
            Name = name;
            Description = description;
            Value = value;
        }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; protected set; }
        [JsonProperty] public int Value { get; protected set; } = 0;
    }

    public class Resistance : Modifiable<BaseResistance>
    {
        public Resistance() { }
        public Resistance(string name, string? description, int value)
        {
            BaseModel = new BaseResistance(name, description, value);
        }

        public Guid Id => BaseModel.Id;
        public string Name => BaseModel.Name;
        public string? Description => BaseModel.Description;
        public int BaseValue => BaseModel.Value;
        public int Value => BaseValue + ModifierRoll("Value");
    }

    public class ResistanceSignature
    {
        [JsonProperty] public Resistance Impact { get; protected set; } = new Resistance();
        [JsonProperty] public Resistance Pierce { get; protected set; } = new Resistance();
        [JsonProperty] public Resistance Blast { get; protected set; } = new Resistance();
        [JsonProperty] public Resistance Burn { get; protected set; } = new Resistance();
        [JsonProperty] public Resistance Energy { get; protected set; } = new Resistance();
    }
}
