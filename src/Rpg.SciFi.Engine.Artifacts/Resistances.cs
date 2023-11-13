using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class BaseResistance
    {
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; protected set; }
        [JsonProperty] public int Value { get; protected set; } = 0;
    }

    public class ImpactResistance : BaseResistance { }
    public class PierceResistance : BaseResistance { }
    public class BlastResistance : BaseResistance { }
    public class BurnResistance : BaseResistance { }
    public class EnergyResistance : BaseResistance { }

    public class Resistance<T> : Modifiable<T> where T : BaseResistance, new()
    {
        public Resistance()
        {
            Name = nameof(Resistance<T>);
        }

        [JsonProperty] public string? Description { get; protected set; }
        public int Value => BaseModel.Value + Modifications.Sum(x => x.DiceEval(this));
    }

    public class ResistanceSignature
    {
        [JsonProperty] public Resistance<ImpactResistance> Impact { get; protected set; } = new Resistance<ImpactResistance>();
        [JsonProperty] public Resistance<PierceResistance> Pierce { get; protected set; } = new Resistance<PierceResistance>();
        [JsonProperty] public Resistance<BlastResistance> Blast { get; protected set; } = new Resistance<BlastResistance>();
        [JsonProperty] public Resistance<BlastResistance> Burn { get; protected set; } = new Resistance<BlastResistance>();
        [JsonProperty] public Resistance<EnergyResistance> Energy { get; protected set; } = new Resistance<EnergyResistance>();
    }
}
