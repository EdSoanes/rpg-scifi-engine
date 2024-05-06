using Rpg.Sys.Components;
using Rpg.Sys.Moddable;
using Rpg.Sys.Modifiers;
using System.Linq.Expressions;

namespace Rpg.Sys.GraphOperations
{
    public class RemoveOp : Operation
    {
        public RemoveOp(Graph graph, ModStore mods, EntityStore entityStore, List<Condition> conditionStore)
            : base(graph, mods, entityStore, conditionStore) { }

        public void Entities(params ModObject[] entities)
        {
            var moddableObjects = new List<ModObject>();
            foreach (var entity in entities)
            {
                var modObjs = entity.Descendants();
                moddableObjects.AddRange(modObjs);
            }

            var modProps = moddableObjects.SelectMany(x => ModStore.Get(x.Id)).ToArray();
            ModStore.Remove(modProps);

            Graph.Notify.Queue(modProps);
            Graph.Notify.Send();

            var entityIds = entities.Select(x => x.Id).ToList();
            var conditions = ConditionStore.Where(x => x.OwningEntityId != null && entityIds.Contains(x.OwningEntityId.Value));
            Conditions(conditions.ToArray());

            foreach (var entity in moddableObjects)
                EntityStore.Remove(entity.Id);
        }

        public void Conditions(params Condition[] conditions)
        {
            var mods = conditions.SelectMany(x => x.GetModifiers()).ToArray();
            Mods(mods);

            foreach (var condition in conditions)
                ConditionStore.Remove(condition);
        }

        public void Mods(params Modifier[] mods) 
        {
            foreach (var mod in mods)
            {
                var modified = ModStore.Iterate(mod, (modProp) =>
                {
                    var removed = modProp.Remove(mod);
                    if (removed != null)
                        Graph.Notify.Queue(modProp);

                    return removed != null;
                });
            }

            Graph.Notify.Send();
        }

        public void Mods<TEntity, TResult>(TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModObject
        {
            var propRef = PropRef.Create(entity, expression);
            if (ModStore.Remove(propRef))
                Graph.Notify.Send(propRef);
        }

        public void Mods<TEntity, TResult>(ModifierType modifierType, TEntity entity, Expression<Func<TEntity, TResult>> expression)
            where TEntity : ModObject
        {
            var propRef = PropRef.Create(entity, expression);
            if (ModStore.Remove(propRef, (mod) => mod.ModifierType == modifierType))
                Graph.Notify.Send(propRef);
        }
    }
}
