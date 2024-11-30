using Rpg.ModObjects.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Activities
{
    internal class ActionTemplateFactory
    {
        public static ActionTemplate[] CreateFor(RpgEntity entity)
        {
            var actions = new List<ActionTemplate>();

            var types = RpgTypeScan.ForSubTypes(typeof(ActionTemplate))
                .Where(x => IsOwnerActionType(entity, x));

            foreach (var type in types)
            {
                var action = (ActionTemplate)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(action.OwnerArchetype!))
                    actions.Add(action);
            }

            return actions.ToArray();
        }

        private static bool IsOwnerActionType(RpgObject entity, Type? actionType)
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
