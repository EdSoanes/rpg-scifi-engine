using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    internal static class Extensions
    {
        internal static ModCmdDescriptor[] ModActionDescriptors<T>(this T entity)
            where T : ModObject
        {
            var methods = entity.GetType().GetMethods()
                .Where(x => x.IsModCmdMethod());

            var res = new List<ModCmdDescriptor>();
            foreach (var method in methods)
            {
                var attrs = method.GetCustomAttributes<ModCmdArgAttribute>(true);
                var args = method.GetParameters()
                    .Select(x => GetModCmdArg(x, attrs.ToArray()))
                    .Where(x => x != null)
                    .Cast<ModCmdArg>()
                    .ToArray();

                res.Add(new ModCmdDescriptor(entity.Id, method.Name) { Args = args.ToArray() });
            }

            return res.ToArray();
        }

        private static bool IsModCmdMethod(this MethodInfo method)
        {
            return method.ReturnType == typeof(ModSet)
                && method.GetCustomAttributes<ModCmdAttribute>().Any();
        }

        private static ModCmdArg? GetModCmdArg(this ParameterInfo parameterInfo, ModCmdArgAttribute[] attrs)
        {
            if (string.IsNullOrEmpty(parameterInfo.Name))
                return null;

            var attr = attrs.FirstOrDefault(x => x.Prop == parameterInfo.Name);

            return new ModCmdArg
            {
                Name = parameterInfo.Name,
                DataType = parameterInfo.ParameterType.Name,
                ArgType = attr?.ArgType ?? ModCmdArgType.Any
            };
        }
    }
}
