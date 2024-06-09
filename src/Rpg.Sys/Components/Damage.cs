using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Damage : RpgComponent
    {
        [JsonProperty] public DamageValue Kinetic { get; private set; }
        [JsonProperty] public DamageValue Heat { get; private set; }
        [JsonProperty] public DamageValue Energy { get; private set; }
        [JsonProperty] public DamageValue Chemical { get; private set; }
        [JsonProperty] public DamageValue Radiation { get; private set; }
        [JsonProperty] public DamageValue Cyber { get; private set; }
        [JsonProperty] public DamageValue Mental { get; private set; }

        [JsonConstructor] private Damage() { }

        public Damage(string entityId, string name, DamageTemplate template)
            : base(entityId, name)
        {
            Kinetic = new DamageValue(entityId, nameof(Kinetic), template.Kinetic, template.KineticArmorPenetration, template.KineticRadius);
            Heat = new DamageValue(entityId, nameof(Heat), template.Heat, template.HeatArmorPenetration, template.HeatRadius);
            Energy = new DamageValue(entityId, nameof(Energy), template.Energy, template.EnergyArmorPenetration, template.EnergyRadius);
            Chemical = new DamageValue(entityId, nameof(Chemical), template.Chemical, template.ChemicalArmorPenetration, template.ChemicalRadius);
            Radiation = new DamageValue(entityId, nameof(Radiation), template.Radiation, template.RadiationArmorPenetration, template.RadiationRadius);
            Cyber = new DamageValue(entityId, nameof(Cyber), template.Cyber, template.CyberArmorPenetration, template.CyberRadius);
            Mental = new DamageValue(entityId, nameof(Mental), template.Mental, template.MentalArmorPenetration, template.MentalRadius);
        }
    }
}
