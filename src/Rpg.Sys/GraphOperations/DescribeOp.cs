using Rpg.Sys.Components;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.GraphOperations
{
    public class DescribeOp : Operation
    {
        public DescribeOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public string[] Prop<TEntity, T>(TEntity entity, Expression<Func<TEntity, T>> expression)
            where TEntity : ModObject
        {
            var desc = new List<string>();
            var propRef = PropRef.Create(entity, expression);
            var propRefDesc = DescribePropRef(propRef);
            if (!string.IsNullOrWhiteSpace(propRefDesc))
            {
                desc.Add(propRefDesc);

                var modProp = Graph.Get.ModProp(propRef);
                desc.AddRange(DescribeModProp(modProp!));
            }

            return desc.ToArray();
        }

        private string[] DescribeModProp(ModProp modProp, Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();

            var res = new List<string>();
            var mods = modProp.FilteredModifiers;
            if (mods.All(x => x.ModifierType == ModifierType.Base && x.Name == ModNames.BaseValue))
                return new string[0];

            foreach (var mod in modProp.FilteredModifiers)
            {
                if (idStack.Contains(mod.Id))
                    throw new Exception($"Stack contains id {mod.Id}");
                else
                    idStack.Push(mod.Id);

                res.Add(DescribeModifier(mod, idStack.Count));
                var nextProp = Graph.Get.ModProp(mod.Source);
                if (nextProp != null)
                    res.AddRange(DescribeModProp(nextProp, idStack));

                idStack.Pop();
            }

            return res.ToArray();
        }

        private string DescribeModifier(Modifier mod, int depth)
        {
            var src = mod.SourceDice != null
                ? $"{mod.Name} => {mod.SourceDice}"
                : DescribePropRef(mod.Source);

            var calcDesc = DescribeDiceCalc(mod.DiceCalc);
            if (calcDesc != null)
                src += $" => {calcDesc}() => {Graph.Evaluate.Mod(mod)}";

            return src!.PadLeft((depth * 2) + src.Length);
        }

        private string? DescribePropRef(PropRef? propRef)
        {
            if (propRef != null)
            {
                var entity = Graph.Get.Entity<ModObject>(propRef.EntityId);
                if (entity != null)
                {
                    var parts = new[]
                    {
                        Graph.Get.Entity<ModObject>(propRef.RootEntityId)?.Name ?? entity.Name,
                        propRef.Path,
                        propRef.Prop
                    };

                    var desc = string.Join('.', parts.Where(x => !string.IsNullOrEmpty(x)).Distinct());
                    desc += $" => {entity!.GetModdableValue(propRef.Prop) ?? Dice.Zero}";

                    return desc;
                }
            }

            return null;
        }

        private string? DescribeDiceCalc(ModifierDiceCalc diceCalc)
        {
            if (!diceCalc.IsCalc)
                return null;

            var src = diceCalc.EntityId != null
                ? Graph.Get.Entity<ModObject>(diceCalc.EntityId)?.Name
                : diceCalc.ClassName;

            return $"{src}.{diceCalc.FuncName}".Trim('.');
        }
    }
}
