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

        private Dice _Calculate(IEnumerable<Modifier> mods)
        {
            Dice dice = "0";

            foreach (var mod in mods)
            {
                Dice modDice = mod.SourceDice
                    ?? Graph.Get.Entity<ModdableObject>(mod.Source!.EntityId)?.GetModdableProperty(mod.Source.Prop) 
                    ?? Dice.Zero;

                dice += _ApplyDiceCalc(modDice, mod.DiceCalc);
            }

            return dice;
        }

        private Dice _ApplyDiceCalc(Dice dice, ModifierDiceCalc diceCalc)
        {
            if (!diceCalc.IsCalc)
                return dice;

            if (diceCalc.IsStatic)
                return this.ExecuteFunction<Dice, Dice>($"{diceCalc.ClassName}.{diceCalc.FuncName}", dice);

            var entity = Graph.Get.Entity<ModdableObject>(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }

    }
}
