using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Emission : ModdableObject
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

        [Moddable] public int BaseMin { get => Resolve(); }
        [Moddable] public int BaseMax { get => Resolve(); }
        [Moddable] public int BaseValue { get => Resolve(); }
        [Moddable] public int BaseRadius { get => Resolve(); }

        [Moddable] public int Min { get => Resolve(); }
        [Moddable] public int Max { get => Resolve(); }
        [Moddable] public int Value { get => Resolve(); }
        [Moddable] public int Radius { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseMin, x => x.BaseMin),
                BaseModifier.Create(this, _baseMax, x => x.BaseMax),
                BaseModifier.Create(this, _baseValue, x => x.BaseValue),
                BaseModifier.Create(this, _baseRadius, x => x.BaseRadius),

                BaseModifier.Create(this, x => x.BaseMin, x => x.Min),
                BaseModifier.Create(this, x => x.BaseMax, x => x.Max),
                BaseModifier.Create(this, x => x.BaseValue, x => x.Value),
                BaseModifier.Create(this, x => x.BaseRadius, x => x.Radius)
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
