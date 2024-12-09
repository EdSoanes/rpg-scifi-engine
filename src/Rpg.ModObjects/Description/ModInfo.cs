using Newtonsoft.Json;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Description
{
    public class ModInfo
    {
        [JsonProperty] public string Id { get; internal init; }
        [JsonProperty] public string Name { get; internal init; }
        [JsonProperty] public string ModType { get; internal init; }
        [JsonProperty] public string Behavior { get; internal init; }
        [JsonProperty] public Dice Value { get; internal init; }
        [JsonProperty] public string? ValueFunction { get; internal init; }
        [JsonProperty] public Dictionary<string, object> AdditionalInfo { get; set; } = new();
        [JsonProperty] public PropInfo? Source { get; set; }

        [JsonConstructor] protected ModInfo() { }

        internal ModInfo(Mod mod, Dice? value)
        {
            Id = mod.Id;
            Name = mod.Name;
            ModType = mod.GetType().Name;
            Behavior = mod.Behavior.GetType().Name;
            Value = value ?? Dice.Zero;
            ValueFunction = mod.SourceValueFunc?.FullName;
        }

        public override string ToString()
        {
            var src = Source != null
                ? $"{Source.Name}.{Source.Prop}"
                : string.Empty;

            var type = Behavior == "Threshold" ? Behavior : ModType;
            return $"{Value} <= ({type}) {src}";
        }
    }
}
