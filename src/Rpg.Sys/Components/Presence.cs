using Newtonsoft.Json;
using Rpg.Sys.Components.Values;

namespace Rpg.Sys.Components
{
    public class Presence : ModdableObject
    {
        [JsonProperty] public int Size { get; private set; }
        [JsonProperty] public int Weight { get; private set; }
        [JsonProperty] public PresenceValue Sound { get; private set; }
        [JsonProperty] public PresenceValue Light { get; private set; }
        [JsonProperty] public PresenceValue Heat { get; private set; }
        [JsonProperty] public PresenceValue Chemical { get; private set; }
        [JsonProperty] public PresenceValue Radiation { get; private set; }
        [JsonProperty] public PresenceValue Electromagnetic { get; private set; }

        [JsonConstructor] private Presence() { }

        public Presence(PresenceTemplate template)
        {
            Sound = new PresenceValue(template.SoundMax, template.SoundCurrent, template.SoundRadius);
            Light = new PresenceValue(template.LightMax, template.LightCurrent, template.LightRadius);
            Heat = new PresenceValue(template.HeatMax, template.HeatCurrent, template.HeatRadius);
            Chemical = new PresenceValue(template.ChemicalMax, template.ChemicalCurrent, template.ChemicalRadius);
            Radiation = new PresenceValue(template.RadiationMax, template.RadiationCurrent, template.RadiationRadius);
            Electromagnetic = new PresenceValue(template.ElectromagneticMax, template.ElectromagneticCurrent, template.ElectromagneticRadius);
        }
    }
}
