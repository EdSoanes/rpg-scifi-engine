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

        public Damage(string name, DamageTemplate template)
            : base(name)
        {
            Kinetic = new DamageValue(nameof(Kinetic), template.Kinetic, template.KineticArmorPenetration, template.KineticRadius);
            Heat = new DamageValue(nameof(Heat), template.Heat, template.HeatArmorPenetration, template.HeatRadius);
            Energy = new DamageValue(nameof(Energy), template.Energy, template.EnergyArmorPenetration, template.EnergyRadius);
            Chemical = new DamageValue(nameof(Chemical), template.Chemical, template.ChemicalArmorPenetration, template.ChemicalRadius);
            Radiation = new DamageValue(nameof(Radiation), template.Radiation, template.RadiationArmorPenetration, template.RadiationRadius);
            Cyber = new DamageValue(nameof(Cyber), template.Cyber, template.CyberArmorPenetration, template.CyberRadius);
            Mental = new DamageValue(nameof(Mental), template.Mental, template.MentalArmorPenetration, template.MentalRadius);
        }

        public override void OnBeforeTime(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnBeforeTime(graph, entity);
            Kinetic.OnBeforeTime(graph, entity);
            Heat.OnBeforeTime(graph, entity);
            Energy.OnBeforeTime(graph, entity);
            Chemical.OnBeforeTime(graph, entity);
            Radiation.OnBeforeTime(graph, entity);
            Cyber.OnBeforeTime(graph, entity);
            Mental.OnBeforeTime(graph, entity);
        }
    }
}
