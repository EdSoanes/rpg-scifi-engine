using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public enum ModifierType
    {
        Permanent, //Always applied, such as stat bonuses, skill increases
        Turn, //Duration in number of turns
        Encounter,
        Immediate //Effect is applied immediately and the modifier is removed, such as damage application
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
        [JsonProperty] private int? _diceRoll { get; set; } = null;

        public Modifier(string name, Dice dice, MetaModLocator target)
        {
            Name = name;
            Dice = dice;
            Target = target;
        }

        public Modifier(string name, MetaModLocator source, MetaModLocator target)
        {
            Name = name;
            Source = source;
            Target = target;
        }

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public Dice? Dice { get; private set; }
        [JsonProperty] public MetaModLocator? Source { get; private set; }
        [JsonProperty] public MetaModLocator Target { get; private set; }

        public int Roll()
        {
            if (_diceRoll == null)
                _diceRoll = MetaDiscovery.GetDice(Source)?.Roll() ?? Dice?.Roll();

            return _diceRoll ?? 0;
        }
    }

    public abstract class Modifiable
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(Modifiable);
        [JsonProperty] protected List<Modifier> Modifiers { get; private set; } = new List<Modifier>();

        public Dice ModifierDice(string prop)
        {
            var exprs = Modifiers
                .Where(x => x.Target.Prop == prop)
                .Select(x => x.Dice)
                .Where(x => x != null)
                .Cast<Dice>();

            Dice dice = string.Concat(exprs);

            return dice;
        }

        public int ModifierRoll(string prop)
        {
            return Modifiers.Where(x => x.Target.Prop == prop).Sum(x => x.Roll());
        }

        public void ClearMods()
        {
            Modifiers.Clear();
        }

        public void AddModifier(Modifier mod)
        {
            if (!Modifiers.Any(x => x.Name == mod.Name))
                Modifiers.Add(mod);
        }

        public void RemoveModifier(string name)
        {
            var mod = Modifiers.SingleOrDefault(x => x.Name == name);
            if (mod != null)
                Modifiers.Remove(mod);
        }
    }
}
