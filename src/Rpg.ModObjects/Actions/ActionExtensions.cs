using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.States;

namespace Rpg.ModObjects.Actions
{
    internal static class ReflectionExtensions
    {
        public static bool IsInitiatorActionType<TInitiator>(this Type? actionType, TInitiator entity)
            where TInitiator : RpgEntity
        {
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 2 && entity.GetType().IsAssignableFrom(genericTypes[1]))
                        return true;
                }

                actionType = actionType.BaseType;
            }

            return false;
        }

        public static Action[] CreateOwnerActions<TOwner>(this TOwner entity)
            where TOwner : RpgEntity
        {
            var actions = new List<Action>();

            var types = RpgReflection.ScanForTypes<Action>()
                .Where(x => IsOwnerActionType(entity, x));

            foreach (var type in types)
            {

                var action = (Action)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(action.OwnerArchetype!)) 
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        private static bool IsOwnerActionType<TOwner>(TOwner entity, Type? actionType) 
            where TOwner : RpgEntity
        {
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableFrom(genericTypes[0]))
                        return true;
                }

                actionType = actionType.BaseType;
            }

            return false;
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
