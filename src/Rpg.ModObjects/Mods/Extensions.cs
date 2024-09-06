using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Mods
{
    public static class Extensions
    {
        public static TTarget AddMod<TTarget>(this TTarget target, Mod mod, string targetProp, Dice value, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod.Set(target, targetProp, value, valueFunc);
            target.AddMods(mod);
            return target;
        }

        public static TTarget AddMod<TTarget>(this TTarget target, Mod mod, PropRef targetPropRef, Dice value)
            where TTarget : RpgObject
                => target.AddMod(mod, targetPropRef.Prop, value);

        public static TTarget AddMod<TTarget, TSourceValue>(this TTarget target, Mod mod, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod.Set(target, targetProp, sourceExpr, valueFunc);
            target.AddMods(mod);
            return target;
        }

        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget target, Mod mod, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod.Set(target, targetExpr, dice, valueFunc);
            target.AddMods(mod);
            return target;
        }

        public static TTarget AddMod<TTarget, TTargetVal, TSourceVal>(this TTarget target, Mod mod, Expression<Func<TTarget, TTargetVal>> targetExpr, Expression<Func<TTarget, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            mod.Set(target, targetExpr, target, sourceExpr, valueFunc);
            target.AddMods(mod);
            return target;
        }

        public static TTarget AddMod<TTarget, TTargetVal, TSource, TSourceVal>(this TTarget target, Mod mod, Expression<Func<TTarget, TTargetVal>> targetExpr, TSource source, Expression<Func<TSource, TSourceVal>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            mod.Set(target, targetExpr, source, sourceExpr, valueFunc);
            target.AddMods(mod);
            return target;
        }
    }
}
