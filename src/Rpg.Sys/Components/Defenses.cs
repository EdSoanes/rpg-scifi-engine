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

        public Defenses(string name, DefensesTemplate template)
            : base(name)
        {
            Kinetic = new DefenseValue(nameof(Kinetic), template.Kinetic, template.KineticShielding);
            Heat = new DefenseValue(nameof(Heat), template.Heat, template.HeatShielding);
            Energy = new DefenseValue(nameof(Energy), template.Energy, template.EnergyShielding);
            Chemical = new DefenseValue(nameof(Chemical), template.Chemical, template.ChemicalShielding);
            Radiation = new DefenseValue(nameof(Radiation), template.Radiation, template.RadiationShielding);
            Cyber = new DefenseValue(nameof(Cyber), template.Cyber, template.CyberShielding);
            Mental = new DefenseValue(nameof(Mental), template.Mental, template.MentalShielding);
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
