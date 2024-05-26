using System.Reflection;

namespace Rpg.ModObjects.Cmds
{
    internal static class ReflectionExtensions
    { 
        internal static ModCmd[] CreateModCommands<T>(this T entity)
            where T : ModObject
        {
            var methods = entity.GetType().GetMethods()
                .Where(x => x.IsModCmdMethod());

            var res = new List<ModCmd>();
            foreach (var method in methods)
            {
                var cmdAttr = method.GetCustomAttributes<ModCmdAttribute>(true).FirstOrDefault();
                var attrs = method.GetCustomAttributes<ModCmdArgAttribute>(true);
                var args = method.GetParameters()
                    .Select(x => ModCmdArg.Create(x, attrs?.FirstOrDefault(a => a.Prop == x.Name)))
                    .Where(x => x != null)
                    .Cast<ModCmdArg>()
                    .ToArray();

                res.Add(ModCmd.Create(entity.Id, method.Name, cmdAttr!, args));
            }

            return res.ToArray();
        }

        private static bool IsModCmdMethod(this MethodInfo method)
        {
            return method.ReturnType.IsAssignableTo(typeof(ModSet))
                && method.GetCustomAttributes<ModCmdAttribute>().Any();
        }
    }
}
