using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rpg.Experimental.States;

namespace Rpg.Experimental.Graph
{
    public class RpgStateCreator
    {
        public State[] CreateStates()
        {

        }

        private void OnCreatingStates()
        {
            var types = RpgTypeScan.ForTypes<State>()
                .Where(x => IsOwnerStateType(this, x));

            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [this])!;
                if (this.IsA(state.OwnerArchetype!))
                {
                    state.OnCreating(Graph);
                    States.Add(state.Name, state);
                }
            }
        }

        private bool IsOwnerStateType(RpgObject entity, Type? stateType)
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
