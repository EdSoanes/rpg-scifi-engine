using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Defenses : RpgComponent
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

        public Defenses(string entityId, string name, DefensesTemplate template)
            : base(entityId, name)
        {
            Kinetic = new DefenseValue(entityId, nameof(Kinetic), template.Kinetic, template.KineticShielding);
            Heat = new DefenseValue(entityId, nameof(Heat), template.Heat, template.HeatShielding);
            Energy = new DefenseValue(entityId, nameof(Energy), template.Energy, template.EnergyShielding);
            Chemical = new DefenseValue(entityId, nameof(Chemical), template.Chemical, template.ChemicalShielding);
            Radiation = new DefenseValue(entityId, nameof(Radiation), template.Radiation, template.RadiationShielding);
            Cyber = new DefenseValue(entityId, nameof(Cyber), template.Cyber, template.CyberShielding);
            Mental = new DefenseValue(entityId, nameof(Mental), template.Mental, template.MentalShielding);
        }
    }
}
