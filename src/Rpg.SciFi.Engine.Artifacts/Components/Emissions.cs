using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Emission : Modifiable
    {
        public Emission() 
        {
            Name = nameof(Emission);
        }

        public Emission(string name)
        {
            Name = name;
        }

        public Emission(string name, int baseValue)
            : this(name)
        {
            BaseValue = baseValue;
        }

        public Emission(string name, int baseMin, int baseMax, int baseValue, int baseRadius)
        {
            Name = name;
            BaseMin = baseMin;
            BaseMax = baseMax;
            BaseValue = baseValue;
            BaseRadius = baseRadius;
        }

        [JsonProperty] public int BaseMin { get; private set; } = 0;
        [JsonProperty] public int BaseMax { get; private set; } = 100;
        [JsonProperty] public int BaseValue { get; private set; } = 0;
        [JsonProperty] public int BaseRadius { get; private set; } = 100;

        [Moddable] public int Min => BaseMin + ModifierRoll(nameof(Min));
        [Moddable] public int Max => BaseMax + ModifierRoll(nameof(Max));
        [Moddable] public int Value => BaseValue + ModifierRoll(nameof(Value));
        [Moddable] public int Radius => BaseRadius + ModifierRoll(nameof(Radius));
    }

    public class EmissionSignature
    {
        public EmissionSignature(
            Emission? visibleLight = null, 
            Emission? heat = null, 
            Emission? radiation = null, 
            Emission? sound = null, 
            Emission? electromagnetic = null)
        {
            VisibleLight = visibleLight ?? new Emission(nameof(VisibleLight));
            Heat = heat ?? new Emission(nameof(Heat));
            Radiation = radiation ?? new Emission(nameof(Radiation));
            Sound = sound ?? new Emission(nameof(Sound));
            Electromagnetic = electromagnetic ?? new Emission(nameof(Electromagnetic));
        }

        [JsonProperty] public Emission VisibleLight { get; protected set; }
        [JsonProperty] public Emission Heat { get; protected set; }
        [JsonProperty] public Emission Radiation { get; protected set; }
        [JsonProperty] public Emission Sound { get; protected set; }
        [JsonProperty] public Emission Electromagnetic { get; protected set; }
    }
}
