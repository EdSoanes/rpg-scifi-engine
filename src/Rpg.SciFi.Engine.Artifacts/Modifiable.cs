using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
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

    public class DeleteCondition
    {
        public Guid EntityId { get; set; }
        public string State { get; set; }
        public bool Not { get; set; }
        public Property Property { get; set; }
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
            DurationType = ModDurationType.Permanent;
            Dice = dice;
            Target = target;
        }

        public Modifier(string name, MetaModLocator source, MetaModLocator target)
        {
            Name = name;
            DurationType = ModDurationType.Permanent;
            Source = source;
            Target = target;
        }

        public Modifier(string name, ModDurationType durationType, Dice dice, MetaModLocator target)
        {
            Name = name;
            DurationType = durationType;
            Dice = dice;
            Target = target;
        }

        public Modifier(string name, ModDurationType durationType, MetaModLocator source, MetaModLocator target)
        {
            Name = name;
            DurationType = durationType;
            Source = source;
            Target = target;
        }

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public ModDurationType DurationType { get; private set; }
        [JsonProperty] public Dice? Dice { get; private set; }
        [JsonProperty] public MetaModLocator? Source { get; private set; }
        [JsonProperty] public MetaModLocator Target { get; private set; }

        public Dice Evaluate()
        {
            if (Dice != null)
                return Dice.Value;

            if (Source?.Id == null)
                return "0";

            var dice = Source.Id.Meta().Evaluate(Source.Prop);
            return dice;
        }
    }

    public abstract class Modifiable : Entity
    {
        [JsonProperty] public string Name { get; protected set; } = nameof(Modifiable);
    }
}
