using Rpg.Experimental.Activities;
using Rpg.Experimental.Reflection;

namespace Rpg.Experimental.Graph
{
    public class RpgActionCreator
    {
        public RpgAction[] CreateActions(RpgObject obj)
        {
            var actions = new List<RpgAction>();

            var types = RpgTypeUtilities.ForSubTypes(typeof(RpgAction))
                .Where(x => IsOwnerActionType(obj, x));

            foreach (var type in types)
            {
                var action = (RpgAction)Activator.CreateInstance(type, [obj])!;
                if (obj.IsA(action.OwnerArchetype!))
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        private bool IsOwnerActionType(RpgObject entity, Type? actionType)
        {
            while (actionType != null)
            {
                if (actionType.IsGenericType)
                {
                    var genericTypes = actionType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                actionType = actionType.BaseType;
            }

            return false;
        }
    }
}
