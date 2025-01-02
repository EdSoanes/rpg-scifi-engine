using System.Reflection;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.States;

namespace Rpg.Experimental.Graph
{
    public class RpgStateCreator
    {
        public State[] CreateStates(RpgObject owner)
        {
            var types = RpgTypeUtilities.ForTypes<State>()
                .Where(x => IsOwnerStateType(owner, x));

            var states = new List<State>();
            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [owner])!;
                states.Add(state);
            }

            return states.ToArray();
        }

        private bool IsOwnerStateType(RpgObject obj, Type? stateType)
        {
            while (stateType != null)
            {
                if (stateType.IsGenericType)
                {
                    var genericTypes = stateType.GetGenericArguments();
                    if (genericTypes.Length == 1 && obj.GetType().IsAssignableTo(genericTypes[0]))
                        return true;
                }

                stateType = stateType.BaseType;
            }

            return false;
        }
    }
}
