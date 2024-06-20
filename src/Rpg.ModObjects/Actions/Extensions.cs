//using System.Reflection;
//using Rpg.ModObjects.Mods;

//namespace Rpg.ModObjects.Actions
//{
//    internal static class ReflectionExtensions
//    { 
//        internal static RpgAction[] CreateActions<T>(this T entity)
//            where T : RpgObject
//        {
//            var methods = entity.GetType().GetMethods()
//                .Where(x => x.IsActionMethod());

//            var res = new List<RpgAction>();
//            foreach (var method in methods)
//            {
//                var cmdAttr = method.GetCustomAttributes<RpgActionAttribute>(true).FirstOrDefault();
//                var attrs = method.GetCustomAttributes<RpgActionArgAttribute>(true);
//                var args = method.GetParameters()
//                    .Select(x => new RpgActionArg(x, attrs?.FirstOrDefault(a => a.Prop == x.Name)))
//                    .Where(x => x != null)
//                    .Cast<RpgActionArg>()
//                    .ToArray();

//                res.Add(RpgAction.Create(entity.Id, method.Name, cmdAttr!, args));
//            }

//            return res.ToArray();
//        }

//        internal static bool IsActionMethod(this MethodInfo method)
//        {
//            return method.ReturnType.IsAssignableTo(typeof(ModSet))
//                && method.GetCustomAttributes<RpgActionAttribute>().Any();
//        }
//    }
//}
