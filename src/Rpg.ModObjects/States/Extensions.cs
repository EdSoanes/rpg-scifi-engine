using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Mods;
using System.Reflection;

namespace Rpg.ModObjects.States
{
    public static class ModStateExtensions
    {
        internal static ModState[] CreateModStates(this RpgObject entity)
        {
            var methods = entity.GetType().GetMethods()
                .Where(x => x.IsModStateMethod());

            var res = new List<ModState>();
            foreach (var method in methods)
            {
                var stateAttr = method.GetCustomAttributes<ModStateAttribute>(true).First();
                var modState = new ModState(entity.Id, stateAttr.Name ?? method.Name, stateAttr.ActiveWhen, method.Name);

                res.Add(modState);
            }

            return res.ToArray();
        }

        private static bool IsModStateMethod(this MethodInfo method)
        {
            if (method.GetCustomAttributes<ModStateAttribute>().Any())
            {
                var args = method.GetParameters();
                if (args.Count() == 1 && args.First().ParameterType.IsAssignableTo(typeof(ModSet)))
                    return true;
            }

            return false;
        }
    }
}
