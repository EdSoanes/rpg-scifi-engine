using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System.Linq.Expressions;

namespace Rpg.SciFi.Engine.Artifacts
{
    public class PropEvaluator
    {
        private ModStore? _modStore;
        private EntityStore? _entityStore;

        public void Initialize(ModStore modStore, EntityStore entityStore)
        {
            _modStore = modStore;
            _entityStore = entityStore;
        }

        public Dice Evaluate(Guid id, string prop)
        {
            var idStack = new Stack<Guid>();
            var modProp = _modStore!.Get(id, prop);
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

        private Dice Evaluate(IEnumerable<Modifier> mods, Stack<Guid>? idStack)
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
                dice += modDice;

                idStack.Pop();
            }

            return dice;
        }

        public string[] Describe(Modifier modifier, bool addEntityInfo) => Describe(modifier, new Stack<Guid>(), addEntityInfo);

        public string[] Describe(ModdableObject entity, string prop, bool addEntityInfo = false)
        {
            var modProp = _modStore!.Get(entity.Id, prop);

            var res = new List<string> 
            { 
                $"{entity.Name}.{prop} => {Evaluate(entity.Id, prop)}" 
            };

            var idStack = new Stack<Guid>();
            foreach (var mod in modProp!.Modifiers)
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

            var modProp = _modStore!.Get(modifier.Source);
            if (modProp != null && modProp.Modifiers.Any())
            {
                foreach (var mod in modProp.Modifiers)
                    res.AddRange(Describe(mod, idStack, false).Select(x => $"  {x}"));
            }
            else
            {
                var entity = _entityStore.Get(modifier.Source.Id);
                if (entity != null)
                    res.Add($"  {modifier.Source.Prop} => {entity.PropertyValue<int>(modifier.Source.Prop)}");
            }

            return res.ToArray();
        }

        private string DescribeSource(Modifier mod, Stack<Guid> idStack, bool addEntityInfo = false)
        {
            var desc = mod.Source.Describe(_entityStore!, addEntityInfo);
            if (mod.Source.PropType == PropType.Dice)
                desc = $"{mod.Name} => {desc}";
            else
            {
                var dice = GetDice(mod.Source.Id, mod.Source.Prop, PropType.Path, idStack);
                desc += $" => {dice}";
            }

            var diceCalc = mod.DiceCalc.Describe(_entityStore!);
            if (diceCalc != null)
                desc += $" => {diceCalc}() => {Evaluate(new[] { mod })}";

            return desc;
        }

        private Dice GetDice(Guid? id, string prop, PropType propType, Stack<Guid> idStack)
        {
            if (propType == PropType.Dice)
                return prop;

            var modProp = _modStore!.Get(id, prop);
            if (modProp != null && modProp.Modifiers.Any())
                return Evaluate(modProp.Modifiers, idStack);

            var entity = _entityStore!.Get(id);
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

            var entity = _entityStore!.Get(diceCalc.EntityId!.Value);
            if (entity != null)
                return entity.ExecuteFunction<Dice, Dice>(diceCalc.FuncName!, dice);

            return dice;
        }
    }
}
