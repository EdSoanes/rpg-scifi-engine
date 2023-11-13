using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts
{
    public abstract class BaseDamage
    {
        public BaseDamage() { }

        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(BaseEmission);
        [JsonProperty] public string? Description { get; protected set; }
        [JsonProperty] public string DiceExpr { get; protected set; } = "d6";
    }

    public class ImpactDamage : BaseDamage { }
    public class PierceDamage : BaseDamage { }
    public class BlastDamage : BaseDamage { }
    public class BurnDamage : BaseDamage { }
    public class EnergyDamage : BaseDamage { }

    public class Damage<T> : Modifiable<T> where T : BaseDamage, new()
    {
        public Damage()
        {
            Name = nameof(Damage<T>);
        }

        public string? Description => BaseModel.Description;
        public string Value => DiceExpr("Value", BaseModel.DiceExpr);
    }

    public class DamageSignature
    {
        [JsonProperty] public Damage<ImpactDamage> Impact { get; protected set; } = new Damage<ImpactDamage>();
        [JsonProperty] public Damage<PierceDamage> Pierce { get; protected set; } = new Damage<PierceDamage>();
        [JsonProperty] public Damage<BlastDamage> Blast { get; protected set; } = new Damage<BlastDamage>();
        [JsonProperty] public Damage<BlastDamage> Burn { get; protected set; } = new Damage<BlastDamage>();
        [JsonProperty] public Damage<EnergyDamage> Energy { get; protected set; } = new Damage<EnergyDamage>();
    }
}
