using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class MetaModdablePropertyEvaluator
    {
        private readonly MetaModifierStore _modifierStore;
        private readonly MetaEntityStore _entityStore;

        public MetaModdablePropertyEvaluator(MetaModifierStore modifierStore, MetaEntityStore entityStore)
        {
            _modifierStore = modifierStore;
            _entityStore = entityStore;
        }

        public Dice Evaluate(Guid id, string prop)
        {
            var idStack = new Stack<Guid>();
            var modProp = _modifierStore.Get(id, prop);
            return modProp != null
                ? Evaluate(modProp.Modifiers, idStack)
                : GetDice(id, prop, PropType.Dice, idStack);
        }

        public Dice Evaluate(MetaModdableProperty? modProp) => Evaluate(modProp.Modifiers);

        public Dice Evaluate(IEnumerable<Modifier> mods, Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();
            Dice dice = "0";
            foreach (var mod in mods)
            {
                if (idStack.Contains(mod.Id))
                    throw new Exception($"Recursion for mod {mod}");
                idStack.Push(mod.Id);
                var modDice = GetDice(mod.Source.Id, mod.Source.Prop, mod.Source.PropType, idStack);
                modDice = ApplyDiceCalc(modDice, mod.DiceCalc);
                idStack.Pop();
                dice += modDice;
            }

            return dice;
        }

        private Dice GetDice(Guid? id, string prop, PropType propType, Stack<Guid> idStack)
        {
            if (propType == PropType.Dice)
                return prop;

            var modProp = _modifierStore.Get(id, prop);
            if (modProp != null && modProp.Modifiers.Any())
                return Evaluate(modProp.Modifiers, idStack);

            var entity = _entityStore.Get(id);
            if (entity != null)
                return entity.PropertyValue<int>(prop);

            return "0";
        }

        private Dice ApplyDiceCalc(Dice dice, ModifierDiceCalc diceCalc)
        {
            if (!diceCalc.IsCalc)
                return dice;

            if (diceCalc.IsStatic)
                return this.ExecuteFunction<Dice, Dice>($"{diceCalc.ClassName}.{diceCalc.FuncName}", dice);

            var entity = _entityStore.Get(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }
    }
}
