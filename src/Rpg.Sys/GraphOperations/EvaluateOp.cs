using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.GraphOperations
{
    public class EvaluateOp : Operation
    {
        public EvaluateOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public Dice Prop<TEntity>(TEntity entity, Expression<Func<TEntity, Dice>> expression)
            where TEntity : ModdableObject
        {
            var modProp = Graph.Get.ModProp(entity, expression);
            return modProp != null
                ? _Calculate(modProp.FilteredModifiers)
                : Dice.Zero;
        }

        public Dice Base<TEntity, T>(TEntity entity, Expression<Func<TEntity, T>> expression)
            where TEntity : ModdableObject
        {
            var modProp = Graph.Get.ModProp(entity, expression);
            return modProp != null
                ? _Calculate(modProp.BaseModifiers)
                : Dice.Zero;
        }

        public Dice Prop(ModProp? modProp)
        {
            return modProp != null
                ? _Calculate(modProp.FilteredModifiers)
                : Dice.Zero;
        }

        public Dice Mod(params Modifier[] mods)
            => _Calculate(mods);

        public static Dice Mod(ModdableObject rootEntity, string prop)
            => Mod(rootEntity, rootEntity.GetMods(prop));

        public static Dice Mod(ModdableObject rootEntity, Modifier[] mods)
        {
            Dice dice = "0";

            foreach (var mod in mods)
            {
                Dice modDice = mod.Source != null
                    ? rootEntity.PropertyValue<Dice>(mod.Source.Prop)// GetPropValue(mod) ?? Dice.Zero;
                    : mod.SourceDice ?? Dice.Zero;

                object diceCalcEntity = mod.DiceCalc?.EntityId != null
                    ? ((object?)rootEntity.Traverse().FirstOrDefault(x => x.Id == mod.DiceCalc.EntityId.Value)) ?? rootEntity
                    : rootEntity;

                dice += _ApplyDiceCalc(diceCalcEntity, modDice, mod.DiceCalc);
            }

            return dice;
        }

        private Dice _Calculate(IEnumerable<Modifier> mods)
        {
            Dice dice = "0";

            foreach (var mod in mods)
            {
                Dice modDice = mod.SourceDice
                    ?? Graph.Get.Entity<ModdableObject>(mod.Source!.EntityId)?.GetModdableProperty(mod.Source.Prop) 
                    ?? Dice.Zero;

                object diceCalcEntity = mod.DiceCalc?.EntityId != null
                    ? ((object?)Graph.Get.Entity<ModdableObject>(mod.DiceCalc.EntityId!.Value)) ?? this
                    : this;

                dice += _ApplyDiceCalc(diceCalcEntity, modDice, mod.DiceCalc);
            }

            return dice;
        }

        private static Dice _ApplyDiceCalc(object? diceCalcEntity, Dice dice, ModifierDiceCalc? diceCalc)
        {
            if (diceCalc == null || !diceCalc.IsCalc)
                return dice;

            var funcName = diceCalc.IsStatic
                ? $"{diceCalc.ClassName}.{diceCalc.FuncName}"
                : diceCalc.FuncName!;

            return diceCalcEntity.ExecuteFunction<Dice, Dice>(funcName, dice);
        }
    }
}
