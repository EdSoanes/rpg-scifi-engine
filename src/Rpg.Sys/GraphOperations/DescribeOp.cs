using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.GraphOperations
{
    public class DescribeOp : Operation
    {
        public DescribeOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public string[] Prop<TEntity, T>(TEntity entity, Expression<Func<TEntity, T>> expression)
            where TEntity : ModdableObject
        {
            var desc = new List<string>();
            var propRef = PropRef.FromPath(entity, expression);
            var modProp = Graph.Get.ModProp(propRef);
            if (modProp != null)
            {
                desc.Add($"{entity!.Name}.{propRef.Path}.{propRef.Prop} => {entity!.GetModdableProperty(modProp.Prop)}");
                desc.AddRange(_Describe(modProp));
            }

            return desc.ToArray();
        }

        private string[] _Describe(ModProp modProp, Stack<Guid>? idStack = null)
        {
            idStack ??= new Stack<Guid>();

            var res = new List<string>();
            foreach (var mod in modProp.FilteredModifiers)
            {
                if (idStack.Contains(mod.Id))
                    throw new Exception($"Stack contains id {mod.Id}");
                else
                    idStack.Push(mod.Id);

                res.Add(_Describe(mod, idStack.Count));
                var nextProp = Graph.Get.ModProp(mod.Source);
                if (nextProp != null)
                    res.AddRange(_Describe(nextProp, idStack));

                idStack.Pop();
            }

            return res.ToArray();
        }

        private string _Describe(Modifier mod, int depth)
        {
            var src = _Describe(mod.Source);
            if (mod.Source.PropType == PropType.Path)
            {
                var entity = Graph.Get.Entity<ModdableObject>(mod.Source.Id);
                src += $" => {entity!.GetModdableProperty(mod.Source.Prop) ?? Dice.Zero}";
            }
            else
                src = $"{mod.Name} => {mod.Source.Prop}";

            var calcDesc = _Describe(mod.DiceCalc);
            if (calcDesc != null)
                src += $" => {calcDesc}() => {Graph.Evaluate.Mod(mod)}";

            return src.PadLeft((depth * 2) + src.Length);
        }

        private string? _Describe(PropRef propRef)
        {
            if (propRef.PropType == PropType.Path)
            {
                var parts = new[]
                {
                    Graph.Get.Entity<ModdableObject>(propRef.RootId)?.Name,
                    Graph.Get.Entity<ModdableObject>(propRef.Id)?.Name,
                    propRef.Prop
                };

                return string.Join('.', parts.Where(x => !string.IsNullOrEmpty(x)).Distinct());
            }

            return null;
        }

        private string? _Describe(ModifierDiceCalc diceCalc)
        {
            if (!diceCalc.IsCalc)
                return null;

            var src = diceCalc.EntityId != null
                ? Graph.Get.Entity<ModdableObject>(diceCalc.EntityId)?.Name
                : diceCalc.ClassName;

            return $"{src}.{diceCalc.FuncName}".Trim('.');
        }
    }
}
