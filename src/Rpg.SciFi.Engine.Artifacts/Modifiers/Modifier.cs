using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using System.Xml;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
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

    public sealed class ModifierSet
    {
        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public Modifier[] Modifiers { get; private set; }

        [JsonConstructor] private ModifierSet() { }

        public ModifierSet(string name, params Modifier[] modifiers)
        {
            Name = name;
            Modifiers = modifiers;
        }
    }

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

        [JsonProperty] public string Name { get; private set; }
        [JsonProperty] public Dice? Dice { get; private set; }
        [JsonProperty] public ModdableProperty? Source { get; private set; }
        [JsonProperty] public ModdableProperty Target { get; private set; }
        [JsonProperty] public string? DiceCalc { get; private set; }
        [JsonProperty] private ModifierCondition? ModCondition{ get; set; }
        
        public int Resolve(int? diceRoll = null)
        {
            if (diceRoll != null)
                Resolution = diceRoll;
            else
            {
                var dice = Evaluate();
                Resolution = dice.Roll();
            }

            return Resolution.Value;
        }

        public Dice Evaluate()
        {
            Dice dice = SourceDice();
            return Calculate(dice);
        }

        public string[] Describe(bool includeEntityInformation = false)
        {
            var desc = DescribeSource(includeEntityInformation);

            if (includeEntityInformation && Meta.Get(Target.Id)?.Meta != null)
                desc += $" to {DescribeTarget()}";

            var res = new List<string>() { desc };

            if (Source?.Prop != null)
            {
                foreach (var mod in Meta.Get(Source.Id)!.Meta.Mods.Get(Source.Prop))
                    res.AddRange(mod.Describe().Select(x =>  $"  {x}"));
            }
 
            return res.ToArray();
        }

        public override string ToString() => $"{DescribeSource(true)} to {DescribeTarget()}";

        private string DescribeSource(bool includeEntityInformation = false)
        {
            var desc = includeEntityInformation && Source != null
                ? $"{Meta.Get(Source.RootId)!.Name}.{Meta.Get(Source.Id)?.Name ?? Name}.{Source.Prop ?? Source.Method}".Trim('.')
                : Source?.Prop ?? Source?.Method ?? Name;

            desc += $" => {SourceDice()}";

            if (DiceCalc != null)
                desc += $" => {DiceCalc}() => {Evaluate()}";

            return desc;
        }

        private string DescribeTarget() => $"{Meta.Get(Target.RootId)?.Name}.{Meta.Get(Target.Id)!.Name}.{Target.Prop}".Trim('.');

        private Dice Calculate(Dice dice)
        {
            if (string.IsNullOrEmpty(DiceCalc))
                return dice;

            var val = Meta.Get(Source?.Id)?.ExecuteFunction<Dice, Dice>(DiceCalc, dice)
                ?? Meta.Get(Target.Id)?.ExecuteFunction<Dice, Dice>(DiceCalc, dice);

            return val != null ? val.Value : "0";
        }

        private Dice SourceDice()
        {
            Dice dice = Dice
                ?? Meta.Get(Source!.Id).PropertyValue<string>(Source.Prop!)
                ?? "0";

            return dice;
        }
        public Modifier IsState(string state)
        {
            ModCondition = new ModifierCondition(state);
            return this;
        }

        public Modifier IsInstant()
        {
            ModCondition = new ModifierCondition(ModifierConditionType.Instant);
            return this;
        }

        public Modifier IsBase()
        {
            ModCondition = new ModifierCondition(ModifierConditionType.Base);
            return this;
        }

        public void Apply(ModifierStore? modifierStore = null)
        {
            ModCondition ??= new ModifierCondition(ModifierConditionType.Instant);

            if (ModCondition.OnTurn == (int)ModifierConditionType.Instant)
                Resolve();

            modifierStore ??= Meta.Get(Target.Id)!.Meta.Mods;
            modifierStore?.Add(this);
        }

        public void Remove(ModifierStore? modifierStore = null)
        {
            modifierStore ??= Meta.Get(Target.Id)!.Meta.Mods;
            modifierStore?.Remove(this);
        }

        public bool ShouldBeRemoved(int turn)
        {
            if (ModCondition == null)
                return true;

            if (ModCondition.OnTurn == (int)ModifierConditionType.Base)
                return false;

            if (ModCondition.OnTurn == (int)ModifierConditionType.Instant)
                return true;

            if (ModCondition.OnTurn == (int)ModifierConditionType.State && !string.IsNullOrEmpty(ModCondition.State))
            {
                //Check if the specified state is applied to the source
                //source.Id.MetaData().Entity.
            }

            return ModCondition.OnTurn > 0 && ModCondition.OnTurn <= turn;
        }

        public bool CanBeCleared()
        {
            return ModCondition != null && ModCondition.OnTurn != (int)ModifierConditionType.Base;
        }
    }
}
