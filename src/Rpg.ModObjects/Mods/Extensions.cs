using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Mods
{
    public static class Extensions
    {
        public static void InitMod<TTarget>(this TTarget entity, string targetProp, Dice dice)
            where TTarget : RpgObject
        {
            var mod = new InitialMod()
                .SetProps(entity.Id, targetProp, dice)
                .Create();

            entity.AddMods(mod);
        }

        public static TTarget BaseMod<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = new BaseMod()
                .SetProps(entity, targetExpr, dice, valueFunc)
                .Create();
            
            entity.AddMods(mod);

            return entity;
        }

        public static TTarget BaseMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = new BaseMod()
                .SetProps(entity, targetExpr, entity, sourceExpr, valueFunc)
                .Create();

            entity.AddMods(mod);

            return entity;
        }

        public static TTarget AddMod<TTarget>(this TTarget entity, ModTemplate template, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, dice, valueFunc)
                .Create();

            entity.AddMods(mod);

            return entity;
        }

        public static TTarget AddMod<TTarget, TSourceValue>(this TTarget entity, ModTemplate template, string targetProp, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = template
                .SetProps(entity, targetProp, sourceExpr, valueFunc)
                .Create();

            entity.AddMods(mod);

            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue>(this TTarget entity, ModTemplate template, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod =  template
                .SetProps(entity, targetExpr, dice, valueFunc)
                .Create();

            entity.AddMods(mod);

            return entity;
        }

        public static TTarget AddMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, ModTemplate template, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var mod = template
                .SetProps(entity, targetExpr, entity, sourceExpr, valueFunc)
                .Create();

            entity.AddMods(mod);

            return entity;
        }
    }
}
