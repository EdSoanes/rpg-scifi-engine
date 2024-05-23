using Rpg.ModObjects.Actions;
using System.Reflection;

namespace Rpg.ModObjects.States
{
    public static class ModStateExtensions
    {
        internal static ModState<T>[] CreateModStates<T>(this T entity)
            where T : ModObject
        {
            var methods = entity.GetType().GetMethods()
                .Where(x => x.IsModStateMethod());

            var res = new List<ModState<T>>();
            foreach (var method in methods)
            {
                var modState = CreateModState(entity, method);
                if (modState != null)
                    res.Add(modState);
            }

            return res.ToArray();
        }

        private static bool IsModStateMethod(this MethodInfo method)
        {
            if (method.GetCustomAttributes<ModStateAttribute>().Any())
            {
                var args = method.GetParameters();
                if (args.Count() == 1 && args.First().ParameterType == typeof(ModSet))
                    return true;
            }

            return false;
        }

        private static ModState<T> CreateModState<T>(T entity, MethodInfo method)
            where T : ModObject
        {
            var stateAttr = method.GetCustomAttributes<ModStateAttribute>(true).First();
            var state = new ModState<T>(entity.Id, stateAttr.Name ?? method.Name, stateAttr.ShouldActivateMethod, method.Name);

            return state;
        }
    }
}
