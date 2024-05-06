using Newtonsoft.Json;
using Rpg.Sys.Components.Values;
using Rpg.Sys.Moddable;

namespace Rpg.Sys.Components
{
    public class Presence : ModObject
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

        public Presence(PresenceTemplate template)
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
    }
}
