using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Actions
{
    internal static class ReflectionExtensions
    {
        public static Action[] CreateActions<T>(this T entity)
            where T : RpgEntity
        {
            var types = RpgReflection.ScanForTypes<Action>()
                .Where(x => x.BaseType!.IsGenericType && entity.GetType().IsAssignableFrom(x.BaseType!.GenericTypeArguments[0]));

            var actions = new List<Action>();

            foreach (var type in types)
            {
                var action = (Action)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(action.OwnerArchetype!)) 
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        public static State[] CreateStateActions<T>(this T entity, Action[] actions)
        where T : RpgEntity
        {
            var actionStates = new List<State>();
            foreach (var action in actions)
            {
                var actionState = new ActionState(entity, action.Name);
                actionStates.Add(actionState);
            }

            return actionStates.ToArray();
        }
    }
}
