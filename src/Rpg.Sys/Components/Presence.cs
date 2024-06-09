using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.Sys.Components.Values;

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

        public Presence(string entityId, string name, PresenceTemplate template)
            : base(entityId, name)
        {
            Size = template.Size;
            Weight = template.Weight;
            Sound = new PresenceValue(entityId, nameof(Sound), template.SoundMax, template.SoundCurrent, template.SoundRadius);
            Light = new PresenceValue(entityId, nameof(Light), template.LightMax, template.LightCurrent, template.LightRadius);
            Heat = new PresenceValue(entityId, nameof(Heat), template.HeatMax, template.HeatCurrent, template.HeatRadius);
            Chemical = new PresenceValue(entityId, nameof(Chemical), template.ChemicalMax, template.ChemicalCurrent, template.ChemicalRadius);
            Radiation = new PresenceValue(entityId, nameof(Radiation), template.RadiationMax, template.RadiationCurrent, template.RadiationRadius);
            Electromagnetic = new PresenceValue(entityId, nameof(Electromagnetic), template.ElectromagneticMax, template.ElectromagneticCurrent, template.ElectromagneticRadius);
        }
    }
}
