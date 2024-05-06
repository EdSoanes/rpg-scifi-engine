using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using Rpg.Sys.Moddable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.Sys.Components
{
    public class Defenses : ModObject
    {
        [JsonProperty] public DefenseValue Kinetic { get; private set; }
        [JsonProperty] public DefenseValue Heat { get; private set; }
        [JsonProperty] public DefenseValue Energy { get; private set; }
        [JsonProperty] public DefenseValue Chemical { get; private set; }
        [JsonProperty] public DefenseValue Radiation { get; private set; }
        [JsonProperty] public DefenseValue Cyber { get; private set; }
        [JsonProperty] public DefenseValue Mental { get; private set; }

        [JsonProperty] public int Evasion { get; protected set; }
        [JsonProperty] public int Concealment { get; protected set; }

        [JsonConstructor] private Defenses() { }

        public Defenses(DefensesTemplate template)
        {
            Kinetic = new DefenseValue(template.Kinetic, template.KineticShielding);
            Heat = new DefenseValue(template.Heat, template.HeatShielding);
            Energy = new DefenseValue(template.Energy, template.EnergyShielding);
            Chemical = new DefenseValue(template.Chemical, template.ChemicalShielding);
            Radiation = new DefenseValue(template.Radiation, template.RadiationShielding);
            Cyber = new DefenseValue(template.Cyber, template.CyberShielding);
            Mental = new DefenseValue(template.Mental, template.MentalShielding);
        }
    }
}
