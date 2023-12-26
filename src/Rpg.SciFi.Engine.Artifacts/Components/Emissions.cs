using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Emission : Entity
    {
        private readonly int _baseMin = 0;
        private readonly int _baseMax = 100;
        private readonly int _baseValue = 0;
        private readonly int _baseRadius = 100;

        public Emission(string name, int baseValue)
        {
            Name = name;
            _baseValue = baseValue;
        }

        public Emission(string name, int baseMin, int baseMax, int baseValue, int baseRadius)
            : this(name, baseValue)
        {
            _baseMin = baseMin;
            _baseMax = baseMax;
            _baseRadius = baseRadius;
        }

        [Moddable] public int BaseMin { get => this.Resolve(nameof(BaseMin)); }
        [Moddable] public int BaseMax { get => this.Resolve(nameof(BaseMax)); }
        [Moddable] public int BaseValue { get => this.Resolve(nameof(BaseValue)); }
        [Moddable] public int BaseRadius { get => this.Resolve(nameof(BaseRadius)); }

        [Moddable] public int Min { get => this.Resolve(nameof(Min)); }
        [Moddable] public int Max { get => this.Resolve(nameof(Max)); }
        [Moddable] public int Value { get => this.Resolve(nameof(Value)); }
        [Moddable] public int Radius { get => this.Resolve(nameof(Radius)); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseMin, x => x.BaseMin),
                BaseModifier.Create(this, _baseMax, x => x.BaseMax),
                BaseModifier.Create(this, _baseValue, x => x.BaseValue),
                BaseModifier.Create(this, _baseRadius, x => x.BaseRadius),

                BaseModifier.Create(this, x => x.BaseMin, x => x.BaseMin),
                BaseModifier.Create(this, x => x.BaseMax, x => x.BaseMax),
                BaseModifier.Create(this, x => x.BaseValue, x => x.BaseValue),
                BaseModifier.Create(this, x => x.BaseRadius, x => x.BaseRadius)
            };
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
