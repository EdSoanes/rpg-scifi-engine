using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Artifacts
{
    public enum ModDurationType
    {
        Permanent,
        Turn,
        Encounter,
        Conditional
    }

    public enum DeleteOn
    {
        Never,
        EndOfThisTurn,
        StartOfNextTurn,
        StartOfTurnNo,
        EndOfTurnNo,
        EndOfEncounter,
        Encounter,
        ConditionsMet,
    }

    public enum ModType
    {
        Base,
        User,
        Instant,
        Permanent,
        Conditional
    }

    public class DeleteCondition
    {
        public Guid EntityId { get; set; }
        public string State { get; set; }
        public bool Not { get; set; }
        public int PropertyValue { get; set; }
    }

    public class DeleteRule
    {
        public DeleteOn DeleteOn { get; set; } = DeleteOn.Never;
        public int OnTurnNo { get; set; } = 0;
        public DeleteCondition[] DeleteConditions { get; set; } = new DeleteCondition[0];
    }

    public sealed class Modifier
    {
        public Modifier(string name, Dice dice, MetaModLocator target)
        {
            Name = name;
            Type = ModType.Base;
            Dice = dice;
            Target = target;
        }

        public Modifier(string name, MetaModLocator source, MetaModLocator target)
        {
            Name = name;
            Type = ModType.Base;
            Source = source;
            Target = target;
        }

        public Modifier(string name, ModType type, Dice dice, MetaModLocator target)
        {
            Name = name;
            Type = type;
            Dice = dice;
            Target = target;
        }

        public Modifier(string name, ModType type, MetaModLocator source, MetaModLocator target)
        {
            Name = name;
            Type = type;
            Source = source;
            Target = target;
        }

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public ModType Type { get; private set; }
        [JsonProperty] public Dice? Dice { get; private set; }
        [JsonProperty] public MetaModLocator? Source { get; private set; }
        [JsonProperty] public MetaModLocator Target { get; private set; }

        public Dice Evaluate()
        {
            if (Dice != null)
                return Dice.Value;

            if (Source?.Id == null)
                return "0";

            var dice = Source.Id.MetaData().Evaluate(Source.Prop);
            return dice;
        }
    }
}
