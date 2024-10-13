using Rpg.ModObjects;
using Rpg.Sys.Components.Values;
using System.Runtime.ConstrainedExecution;
using Newtonsoft.Json;

namespace Rpg.Sys.Components
{
    public class Presence : RpgComponent
    {
        [JsonProperty] public int Size { get; protected set; }
        [JsonProperty] public int Weight { get; protected set; }
        [JsonProperty] public PresenceValue Sound { get; private set; }
        [JsonProperty] public PresenceValue Light { get; private set; }
        [JsonProperty] public PresenceValue Heat { get; private set; }
        [JsonProperty] public PresenceValue Chemical { get; private set; }
        [JsonProperty] public PresenceValue Radiation { get; private set; }
        [JsonProperty] public PresenceValue Electromagnetic { get; private set; }

        [JsonConstructor] private Presence() { }

        public Presence(string name, PresenceTemplate template)
            : base(name)
        {
            Size = template.Size;
            Weight = template.Weight;
            Sound = new PresenceValue(nameof(Sound), template.SoundMax, template.SoundCurrent, template.SoundRadius);
            Light = new PresenceValue(nameof(Light), template.LightMax, template.LightCurrent, template.LightRadius);
            Heat = new PresenceValue(nameof(Heat), template.HeatMax, template.HeatCurrent, template.HeatRadius);
            Chemical = new PresenceValue(nameof(Chemical), template.ChemicalMax, template.ChemicalCurrent, template.ChemicalRadius);
            Radiation = new PresenceValue(nameof(Radiation), template.RadiationMax, template.RadiationCurrent, template.RadiationRadius);
            Electromagnetic = new PresenceValue(nameof(Electromagnetic), template.ElectromagneticMax, template.ElectromagneticCurrent, template.ElectromagneticRadius);
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Sound.OnCreating(graph, entity);
            Heat.OnCreating(graph, entity);
            Light.OnCreating(graph, entity);
            Chemical.OnCreating(graph, entity);
            Radiation.OnCreating(graph, entity);
            Electromagnetic.OnCreating(graph, entity);
        }
    }
}
