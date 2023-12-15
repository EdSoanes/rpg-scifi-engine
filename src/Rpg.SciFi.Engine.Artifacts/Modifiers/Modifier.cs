using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public sealed class Modifier
    {
        [JsonProperty] private int? Resolution { get; set; }

        [JsonConstructor] private Modifier() { }

        internal Modifier(string name, Dice dice, ModdableProperty target, string? diceCalc = null)
        {
            Name = name;
            Dice = dice;
            Target = target;
            DiceCalc = diceCalc;
        }

        internal Modifier(ModdableProperty source, ModdableProperty target, string? diceCalc = null)
        {
            Name = source.Source;
            Source = source;
            Target = target;
            DiceCalc = diceCalc;
        }

        internal Modifier(string? name, ModdableProperty source, ModdableProperty target, string? diceCalc = null)
        {
            Name = name ?? source.Source;
            Source = source;
            Target = target;
            DiceCalc = diceCalc;
        }

        [JsonProperty] public Guid Id { get; private set; } = Guid.NewGuid();
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public Dice? Dice { get; private set; }
        [JsonProperty] public ModdableProperty? Source { get; private set; }
        [JsonProperty] public ModdableProperty Target { get; private set; }
        [JsonProperty] public string? DiceCalc { get; private set; }
        [JsonProperty] private ModifierCondition? ModCondition{ get; set; } = new ModifierCondition(ModifierType.Base);

        public Modifier IsState(string state)
        {
            ModCondition = new ModifierCondition(state);
            return this;
        }

        public Modifier IsAdditive()
        {
            ModCondition = new ModifierCondition(ModifierType.Additive);
            return this;
        }

        public Modifier IsSubtractive()
        {
            ModCondition = new ModifierCondition(ModifierType.Subtractive);
            return this;
        }

        public Modifier IsBase()
        {
            ModCondition = new ModifierCondition(ModifierType.Base);
            return this;
        }

        public Modifier IsCustom()
        {
            ModCondition = new ModifierCondition(ModifierType.Custom);
            return this;
        }

        public Modifier IsPlayer()
        {
            ModCondition = new ModifierCondition(ModifierType.Player);
            return this;
        }

        public Modifier UntilTurn(int untilTurn)
        {
            ModCondition = new ModifierCondition(untilTurn);
            return this;
        }

        public Modifier UntilEncounterEnds()
        {
            ModCondition = new ModifierCondition(ModifierConditional.Encounter);
            return this;
        }
        //public void Apply(ModifierStore? modifierStore = null)
        //{
        //    ModCondition ??= new ModifierCondition(ModifierType.Additive);

        //    modifierStore ??= Meta.Get(Target.Id)!.MetaData.Mods;
        //    modifierStore?.Add(this);
        //}

        //public void Remove(ModifierStore? modifierStore = null)
        //{
        //    modifierStore ??= Meta.Get(Target.Id)!.MetaData.Mods;
        //    modifierStore?.Remove(this);
        //}

        public bool ShouldBeRemoved(int turn)
        {
            if (ModCondition == null)
                return true;

            if (ModCondition.Condition == ModifierConditional.Encounter)
                return turn <= 0;

            if (ModCondition.Condition == ModifierConditional.Turn)
                return turn >= ModCondition.UntilTurn;

            if (ModCondition.Condition == ModifierConditional.State)
            {
                //Check if the specified target entity has the specified state applied
                //source.Id.MetaData().Entity.
            }

            return false;
        }

        public bool CanBeCleared()
        {
            return ModCondition != null && ModCondition.Condition != ModifierConditional.Permanent;
        }
    }
}
