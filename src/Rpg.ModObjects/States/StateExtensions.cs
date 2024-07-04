using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.ModObjects.Reflection;

namespace Rpg.ModObjects.States
{
    internal static class StateExtensions
    {
        public static State[] CreateStates<T>(this T entity)
            where T : RpgEntity
        {
            var states = new List<State>();

            var types = RpgReflection.ScanForTypes<State>()
                .Where(x => IsOwnerStateType(entity, x));

            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [entity])!;
                if (entity.IsA(state.OwnerArchetype!))
                    states.Add(state);
            }

            return states.ToArray();
        }

        private static bool IsOwnerStateType<TOwner>(TOwner entity, Type? stateType)
            where TOwner : RpgEntity
        {
            while (stateType != null)
            {
                if (stateType.IsGenericType)
                {
                    var genericTypes = stateType.GetGenericArguments();
                    if (genericTypes.Length == 1 && entity.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                stateType = stateType.BaseType;
            }

            return false;
        }
    }
}
