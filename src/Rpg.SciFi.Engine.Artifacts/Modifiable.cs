using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts
{
    public sealed class Modifier
    {
        private int? _evaluatedDiceRoll = null;
        [JsonProperty] public Guid Id { get; protected set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; set; }
        [JsonProperty] public string Target { get; set; }
        [JsonProperty] public Dice Dice { get; set; }

        public int Roll()
        {
            if (_evaluatedDiceRoll == null)
                _evaluatedDiceRoll = Dice.Roll();

            return _evaluatedDiceRoll.Value;
        }
    }

    public abstract class BaseModifiable
    {
        public BaseModifiable(Guid? id = null, string? name = null)
        {
            Id = id ?? Guid.NewGuid();
            Name = name ?? GetType().Name;
        }

        public Guid Id { get; }
        public string Name { get; }
    }

    public class Modifiable<T> where T : class, new()
    {
        [JsonProperty] protected List<Modifier> Modifiers { get; private set; } = new List<Modifier>();
        [JsonProperty] protected T BaseModel { get; set; } = new T();

        public Dice ModifierDice(string prop)
        {
            var exprs = Modifiers.Where(x => x.Target == prop).Select(x => x.Dice);
            Dice dice = string.Concat(exprs);

            return dice;
        }

        public int ModifierRoll(string prop)
        {
            return Modifiers.Where(x => x.Target == prop).Sum(x => x.Roll());
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
