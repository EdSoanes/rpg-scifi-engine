using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Emission : Entity
    {
        public Emission(string name, int baseValue)
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

        [JsonProperty] public string Name { get; private set; } = nameof(Emission);
        [JsonProperty] public int BaseMin { get; private set; } = 0;
        [JsonProperty] public int BaseMax { get; private set; } = 100;
        [JsonProperty] public int BaseValue { get; private set; } = 0;
        [JsonProperty] public int BaseRadius { get; private set; } = 100;

        [Moddable] public int Min => this.Resolve(nameof(Min));
        [Moddable] public int Max => this.Resolve(nameof(Max));
        [Moddable] public int Value => this.Resolve(nameof(Value));
        [Moddable] public int Radius => this.Resolve(nameof(Radius));

        [Setup]
        public void Setup()
        {
            this.AddBaseMod((e) => e.BaseMin, (e) => e.Min);
            this.AddBaseMod((e) => e.BaseMax, (e) => e.Max);
            this.AddBaseMod((e) => e.BaseValue, (e) => e.Value);
            this.AddBaseMod((e) => e.BaseRadius, (e) => e.Radius);
        }
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
            VisibleLight = visibleLight ?? new Emission(nameof(VisibleLight), 0);
            Heat = heat ?? new Emission(nameof(Heat), 0);
            Radiation = radiation ?? new Emission(nameof(Radiation), 0);
            Sound = sound ?? new Emission(nameof(Sound), 0);
            Electromagnetic = electromagnetic ?? new Emission(nameof(Electromagnetic), 0);
        }

        [JsonProperty] public Emission VisibleLight { get; protected set; }
        [JsonProperty] public Emission Heat { get; protected set; }
        [JsonProperty] public Emission Radiation { get; protected set; }
        [JsonProperty] public Emission Sound { get; protected set; }
        [JsonProperty] public Emission Electromagnetic { get; protected set; }
    }
}
