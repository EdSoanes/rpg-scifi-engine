using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys
{
    public class PropEvaluator
    {
        private readonly Graph _graph;

        public PropEvaluator(Graph graph)
        {
            _graph = graph;
        }

        public Dice Evaluate(Guid id, string prop)
        {
            var idStack = new Stack<Guid>();
            var modProp = _graph.Mods.Get(id, prop);
            return modProp != null
                ? Evaluate(modProp.FilteredModifiers, idStack)
                : GetDice(id, prop, PropType.Dice, idStack);
        }

        public Dice Evaluate<TResult>(ModdableObject entity, Expression<Func<ModdableObject, TResult>> expression)
        {
            var propRef = PropRef.FromPath(entity, expression);
            return entity?.GetModdableProperty(propRef.Prop) ?? Dice.Zero;
        }

        public Dice Evaluate(ModProp? modProp) => Evaluate(modProp!.FilteredModifiers);

        public Dice Evaluate(IEnumerable<Modifier> mods) => Evaluate(mods, new Stack<Guid>());

        public Dice Evaluate(Modifier mod, Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();

            if (idStack.Contains(mod.Id))
                throw new Exception($"Recursion for mod {mod}");

            idStack.Push(mod.Id);

            Dice dice = mod.Source.PropType == PropType.Dice
                ? mod.Source.Prop
                : _graph.Entities.Get(mod.Source.Id)?.GetModdableProperty(mod.Source.Prop) ?? Dice.Zero;

            dice = ApplyDiceCalc(dice, mod.DiceCalc);

            idStack.Pop();

            return dice;
        }

        private Dice Evaluate(IEnumerable<Modifier> mods, Stack<Guid>? idStack)
        {
            idStack ??= new Stack<Guid>();
            Dice dice = "0";

            foreach (var mod in mods)
                dice += Evaluate(mod, idStack);

            return dice;
        }

        public string[] Describe(Modifier modifier, bool addEntityInfo) => Describe(modifier, new Stack<Guid>(), addEntityInfo);

        public string[] Describe(ModdableObject entity, string prop, bool addEntityInfo = false)
        {
            var modProp = _graph.Mods.Get(entity.Id, prop);

            var res = new List<string> 
            { 
                $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" 
            };

            var idStack = new Stack<Guid>();
            foreach (var mod in modProp!.FilteredModifiers)
            {
                if (idStack.Contains(mod.Id))
                    throw new Exception($"Recursion for mod {mod}");
                idStack.Push(mod.Id);

                res.AddRange(Describe(mod, idStack, addEntityInfo));

                idStack.Pop();
            }

            return res.ToArray();
        }

        private string[] Describe(Modifier modifier, Stack<Guid> idStack, bool addEntityInfo)
        {
            var desc = DescribeSource(modifier, idStack, addEntityInfo);
            var res = new List<string>() { desc };

            var modProp = _graph.Mods.Get(modifier.Source);
            if (modProp != null && modProp.FilteredModifiers.Any())
            {
                foreach (var mod in modProp.FilteredModifiers)
                    res.AddRange(Describe(mod, idStack, false).Select(x => $"  {x}"));
            }
            else
            {
                var entity = _graph.Entities.Get(modifier.Source.Id);
                if (entity != null)
                    res.Add($"  {modifier.Source.Prop} => {entity.PropertyValue<int>(modifier.Source.Prop)}");
            }

            return res.ToArray();
        }

        private string DescribeSource(Modifier mod, Stack<Guid> idStack, bool addEntityInfo = false)
        {
            var desc = mod.Source.Describe(_graph.Entities, addEntityInfo);
            if (mod.Source.PropType == PropType.Dice)
                desc = $"{mod.Name} => {desc}";
            else
            {
                var dice = GetDice(mod.Source.Id, mod.Source.Prop, PropType.Path, idStack);
                desc += $" => {dice}";
            }

            var diceCalc = mod.DiceCalc.Describe(_graph.Entities);
            if (diceCalc != null)
                desc += $" => {diceCalc}() => {Evaluate(new[] { mod })}";

            return desc;
        }

        private Dice GetDice(Guid? id, string prop, PropType propType, Stack<Guid> idStack)
        {
            if (propType == PropType.Dice)
                return prop;

            var modProp = _graph.Mods.Get(id, prop);
            if (modProp != null && modProp.Modifiers.Any())
                return Evaluate(modProp.Modifiers, idStack);

            var entity = _graph.Entities.Get(id);
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

            var entity = _graph.Entities.Get(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }
    }
}
