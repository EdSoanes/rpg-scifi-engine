using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class Emission : Modifiable
    {
        public Emission() { }

        public Emission(string name)
        {
            Name = name;
        }

        public Emission(string name, int baseValue)
        {
            Name = name;
            BaseValue = baseValue;
        }

        public Emission(string name, string description, int baseMin, int baseMax, int baseValue, int baseRadius)
        {
            Name = name;
            Description = description;
            BaseMin = baseMin;
            BaseMax = baseMax;
            BaseValue = baseValue;
            BaseRadius = baseRadius;
        }

        [JsonProperty] public int BaseMin { get; private set; } = 0;
        [JsonProperty] public int BaseMax { get; private set; } = 100;
        [JsonProperty] public int BaseValue { get; private set; } = 0;
        [JsonProperty] public int BaseRadius { get; private set; } = 100;

        [Modifiable("Min", "Minimum")] public int Min => BaseMin + ModifierRoll(nameof(Min));
        [Modifiable("Max", "Maximum")] public int Max => BaseMax + ModifierRoll(nameof(Max));
        [Modifiable("Value", "Current Value")] public int Value => BaseValue + ModifierRoll(nameof(Value));
        [Modifiable("Radius", "Current Radius")] public int Radius => BaseRadius + ModifierRoll(nameof(Radius));
    }

    public class EmissionSignature
    {
        [JsonProperty] public Emission VisibleLight { get; protected set; } = new Emission(nameof(VisibleLight));
        [JsonProperty] public Emission Heat { get; protected set; } = new Emission(nameof(Heat));
        [JsonProperty] public Emission Radiation { get; protected set; } = new Emission(nameof(Radiation));
        [JsonProperty] public Emission Sound { get; protected set; } = new Emission(nameof(Sound));
        [JsonProperty] public Emission Electromagnetic { get; protected set; } = new Emission(nameof(Electromagnetic));
    }
}
