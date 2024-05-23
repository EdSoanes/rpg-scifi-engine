using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Reflection;

namespace Rpg.ModObjects.Actions
{
    public static class ModSetExtensions
    {
        public static string Target(ModSet modSet) => $"{modSet.Name}.Target";
        public static string Roll(ModSet modSet) => $"{modSet.Name}.Roll";

        public static ModSet Target<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(Target(modSet), entity, Target(modSet), dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(Target(modSet), entity, Target(modSet), entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Target<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(Target(modSet), target, Target(modSet), source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }


        public static ModSet Roll<TEntity>(this ModSet modSet, TEntity entity, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity>(Roll(modSet), entity, Roll(modSet), dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TEntity, TSourceValue>(this ModSet modSet, TEntity entity, Expression<Func<TEntity, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ExternalMod, TEntity, TEntity, TSourceValue>(Roll(modSet), entity, Roll(modSet), entity, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet Roll<TTarget, TSource, TSourceValue>(this ModSet modSet, TTarget target, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<ExternalMod, TTarget, TSource, TSourceValue>(Roll(modSet), target, Roll(modSet), source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }

    internal static class ReflectionExtensions
    { 
        internal static ModCmd[] ModActionDescriptors<T>(this T entity)
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
                    .Select(x => GetModCmdArg(x, attrs.ToArray()))
                    .Where(x => x != null)
                    .Cast<ModCmdArg>()
                    .ToArray();

                res.Add(new ModCmd(entity.Id, method.Name, cmdAttr?.OutcomeMethod, args.ToArray()));
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
                DataType = parameterInfo.ParameterType.FullName,
                ArgType = attr?.ArgType ?? ModCmdArgType.Any
            };
        }
    }
}
