using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Modifiers
{
    public static class Extensions
    {
        public static TTarget InitMod<TTarget>(this TTarget entity, string targetProp, Dice dice)
            where TTarget : RpgObject
        {
            var template = new InitialMod();
            template.SetProps(entity.Id, targetProp, dice);

            var mod = new Mod(targetProp, template);
            entity.AddMods(mod);

            return entity;
        }

        public static TTarget BaseMod<TTarget, TTargetValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var template = new BaseMod();
            template.SetProps(entity, targetExpr, dice, valueFunc);
            
            var mod = new Mod(template.TargetPropRef.Prop, template);
            entity.AddMods(mod);

            return entity;
        }

        public static TTarget BaseMod<TTarget, TTargetValue, TSourceValue>(this TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Expression<Func<TTarget, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueFunc = null)
            where TTarget : RpgObject
        {
            var template = new BaseMod();
            template.SetProps<TTarget, TTargetValue, TTarget, TSourceValue>(entity, targetExpr, entity, sourceExpr, valueFunc);

            var mod = new Mod(template.TargetPropRef.Prop, template);
            entity.AddMods(mod);

            return entity;
        }

    }
}
