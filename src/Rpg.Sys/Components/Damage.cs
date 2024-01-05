using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Damage
    {
        [JsonProperty] public DamageValue Kinetic { get; private set; }
        [JsonProperty] public DamageValue Heat { get; private set; }
        [JsonProperty] public DamageValue Energy { get; private set; }
        [JsonProperty] public DamageValue Chemical { get; private set; }
        [JsonProperty] public DamageValue Radiation { get; private set; }
        [JsonProperty] public DamageValue Cyber { get; private set; }
        [JsonProperty] public DamageValue Mental { get; private set; }

        [JsonConstructor] private Damage() { }

        public Damage(DamageTemplate template)
        {
            Kinetic = new DamageValue(template.Kinetic, template.KineticArmorPenetration, template.KineticRadius);
            Heat = new DamageValue(template.Heat, template.HeatArmorPenetration, template.HeatRadius);
            Energy = new DamageValue(template.Energy, template.EnergyArmorPenetration, template.EnergyRadius);
            Chemical = new DamageValue(template.Chemical, template.ChemicalArmorPenetration, template.ChemicalRadius);
            Radiation = new DamageValue(template.Radiation, template.RadiationArmorPenetration, template.RadiationRadius);
            Cyber = new DamageValue(template.Cyber, template.CyberArmorPenetration, template.CyberRadius);
            Mental = new DamageValue(template.Mental, template.MentalArmorPenetration, template.MentalRadius);
        }
    }
}
