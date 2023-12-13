using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public enum ModifierType
    {
        Base,
        Player,
        Custom,
        Conditional,
        Additive,
        Subtractive
    }

    public enum ModifierConditional
    {
        Instant,
        Turn,
        Encounter,
        State,
        Permanent
    }

    public class ModifierCondition
    {
        [JsonProperty] public ModifierType Type { get; private set; }
        [JsonProperty] public ModifierConditional Condition { get; private set; }
        [JsonProperty] public string? State { get; private set; }
        [JsonProperty] public int UntilTurn { get; set; }

        [JsonConstructor] private ModifierCondition() { }

        public ModifierCondition(string state)
        {
            State = state;
            Type = ModifierType.Conditional;
            Condition = ModifierConditional.State;
        }

        public ModifierCondition(ModifierType modifierType)
        {
            if (modifierType == ModifierType.Conditional)
                throw new ArgumentException(nameof(modifierType));

            Type = modifierType;
            Condition = modifierType == ModifierType.Additive || modifierType == ModifierType.Subtractive
                ? ModifierConditional.Instant
                : ModifierConditional.Permanent;
        }

        public ModifierCondition(int untilTurn)
        {
            Type = ModifierType.Conditional;
            Condition = ModifierConditional.Turn;
            UntilTurn = untilTurn;
        }

        public ModifierCondition(ModifierConditional conditional)
        {
            if (conditional == ModifierConditional.State)
                throw new ArgumentException(nameof(conditional));

            Type = ModifierType.Conditional;
            Condition = conditional;
        }

    }
}
