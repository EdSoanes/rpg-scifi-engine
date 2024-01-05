using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class PropEvaluator
    {
        private EntityGraph? _graph;

        public void Initialize(EntityGraph graph)
        {
            _graph = graph;
        }

        public Dice Evaluate(Guid id, string prop)
        {
            var idStack = new Stack<Guid>();
            var modProp = _graph!.Mods!.Get(id, prop);
            return modProp != null
                ? Evaluate(modProp.Modifiers, idStack)
                : GetDice(id, prop, PropType.Dice, idStack);
        }

        public Dice Evaluate<TResult>(ModdableObject entity, Expression<Func<ModdableObject, TResult>> expression)
        {
            var propRef = PropRef.FromPath(entity, expression);
            return Evaluate(propRef.Id!.Value, propRef.Prop);
        }

        public Dice Evaluate(ModProp? modProp) => Evaluate(modProp!.Modifiers);

        public Dice Evaluate(IEnumerable<Modifier> mods) => Evaluate(mods, new Stack<Guid>());

        public Dice Evaluate(Modifier mod, Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();

            if (idStack.Contains(mod.Id))
                throw new Exception($"Recursion for mod {mod}");

            idStack.Push(mod.Id);

            var modDice = GetDice(mod.Source.Id, mod.Source.Prop, mod.Source.PropType, idStack);
            var dice = ApplyDiceCalc(modDice, mod.DiceCalc);

            idStack.Pop();

            return dice;
        }

        private Dice Evaluate(IEnumerable<Modifier> mods, Stack<Guid>? idStack)
        {
            idStack ??= new Stack<Guid>();
            Dice dice = "0";
            foreach (var mod in FilteredModifiers(mods))
            {
                dice += Evaluate(mod, idStack);
            }

            return dice;
        }

        public string[] Describe(Modifier modifier, bool addEntityInfo) => Describe(modifier, new Stack<Guid>(), addEntityInfo);

        public string[] Describe(ModdableObject entity, string prop, bool addEntityInfo = false)
        {
            var modProp = _graph!.Mods!.Get(entity.Id, prop);

            var res = new List<string> 
            { 
                $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" 
            };

            var idStack = new Stack<Guid>();
            foreach (var mod in FilteredModifiers(modProp!.Modifiers))
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

            var modProp = _graph!.Mods!.Get(modifier.Source);
            if (modProp != null && modProp.Modifiers.Any())
            {
                foreach (var mod in FilteredModifiers(modProp.Modifiers))
                    res.AddRange(Describe(mod, idStack, false).Select(x => $"  {x}"));
            }
            else
            {
                var entity = _graph!.Entities!.Get(modifier.Source.Id);
                if (entity != null)
                    res.Add($"  {modifier.Source.Prop} => {entity.PropertyValue<int>(modifier.Source.Prop)}");
            }

            return res.ToArray();
        }

        private string DescribeSource(Modifier mod, Stack<Guid> idStack, bool addEntityInfo = false)
        {
            var desc = mod.Source.Describe(_graph!.Entities!, addEntityInfo);
            if (mod.Source.PropType == PropType.Dice)
                desc = $"{mod.Name} => {desc}";
            else
            {
                var dice = GetDice(mod.Source.Id, mod.Source.Prop, PropType.Path, idStack);
                desc += $" => {dice}";
            }

            var diceCalc = mod.DiceCalc.Describe(_graph!.Entities!);
            if (diceCalc != null)
                desc += $" => {diceCalc}() => {Evaluate(new[] { mod })}";

            return desc;
        }

        private Dice GetDice(Guid? id, string prop, PropType propType, Stack<Guid> idStack)
        {
            if (propType == PropType.Dice)
                return prop;

            var modProp = _graph!.Mods!.Get(id, prop);
            if (modProp != null && modProp.Modifiers.Any())
                return Evaluate(modProp.Modifiers, idStack);

            var entity = _graph!.Entities!.Get(id);
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

            var entity = _graph!.Entities!.Get(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }

        private Modifier[] FilteredModifiers(IEnumerable<Modifier> modifiers)
        {
            var res = modifiers
                .Where(x => x.ModifierType != ModifierType.Base && x.ModifierType != ModifierType.Player)
                .ToList();

            var baseMods = modifiers
                .Except(res)
                .GroupBy(x => $"{x.Target.Id}.{x.Target.Prop}.{x.Name}");

            foreach (var group in baseMods)
            {
                var mod = group.FirstOrDefault(x => x.ModifierType == ModifierType.Player) ?? group.FirstOrDefault(x => x.ModifierType == ModifierType.Base);
                if (mod != null)
                    res.Add(mod);
            }

            return res.ToArray();
        }
    }
}
