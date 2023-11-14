using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public sealed class Modifier
    {
        [JsonProperty] private int? _diceRoll { get; set; } = null;

        [JsonProperty] public Guid Id { get; private set;} = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public Property Property { get; set; }
        [JsonProperty] public Dice Dice { get; set; }

        public int Roll()
        {
            if (_diceRoll == null)
                _diceRoll = Dice.Roll();

            return _diceRoll.Value;
        }
    }

    public abstract class Modifiable
    {
        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; protected set; } = nameof(Modifiable);
        [JsonProperty] public string? Description { get; protected set; }

        [JsonProperty] protected List<Modifier> Modifiers { get; private set; } = new List<Modifier>();

        public Dice ModifierDice(string prop)
        {
            var exprs = Modifiers.Where(x => x.Property.Prop == prop).Select(x => x.Dice);
            Dice dice = string.Concat(exprs);

            return dice;
        }

        public int ModifierRoll(string prop)
        {
            return Modifiers.Where(x => x.Property.Prop == prop).Sum(x => x.Roll());
        }

        public void ClearMods()
        {
            Modifiers.Clear();
        }

        public void AddModifier(Modifier mod)
        {
            if (!Modifiers.Any(x => x.Id == mod.Id))
                Modifiers.Add(mod);
        }

        public void RemoveModifier(Guid id)
        {
            var mod = Modifiers.SingleOrDefault(x => x.Id == id);
            if (mod != null)
                Modifiers.Remove(mod);
        }
    }
}
