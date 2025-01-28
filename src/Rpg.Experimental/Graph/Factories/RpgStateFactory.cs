using System.Reflection;
using Rpg.Experimental.Reflection;

namespace Rpg.Experimental.Graph.Factories
{
    public class RpgStateFactory
    {
        public RpgState[] CreateStates(RpgObject owner)
        {
            var types = RpgTypeUtilities.ForTypes<RpgState>()
                .Where(x => IsOwnerStateType(owner, x));

            var states = new List<RpgState>();
            foreach (var type in types)
            {
                var state = (RpgState)Activator.CreateInstance(type, [owner])!;
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
