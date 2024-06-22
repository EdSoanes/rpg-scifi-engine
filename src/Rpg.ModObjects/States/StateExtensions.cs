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
            var types = RpgReflection.ScanForTypes<State>()
                .Where(x => x.BaseType!.IsGenericType && entity.GetType().IsAssignableFrom(x.BaseType!.GenericTypeArguments[0]));

            var states = new List<State>();

            foreach (var type in types)
            {
                var state = (State)Activator.CreateInstance(type, [entity])!;
                states.Add(state);
            }
            return states.ToArray();
        }
    }
}
