using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects.Actions
{
    public class ActivityModSet : ModSetBase
    {
        public ActivityModSet Add<TTarget>(string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
                => _Add<TTarget>(ModType.Base, targetProp, dice, valueCalc);

        public ActivityModSet AddResult<Target>(string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where Target : RpgObject
                => _Add<Target>(ModType.Override, targetProp, dice, valueCalc);

        private ActivityModSet _Add<TOwner>(ModType modType, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TOwner : RpgObject
        {
            var entity = Graph!.GetObject<TOwner>(OwnerId)!;
            var mod = new SyncedMod(Id, modType)
                .SetProps(entity, PropName(targetProp), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }




        public ActivityModSet Add<TTarget>(TTarget target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
                => _Add(ModType.Base, target, targetProp, dice, valueCalc);

        public ActivityModSet AddResult<TTarget>(TTarget target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
                => _Add(ModType.Override, target, targetProp, dice, valueCalc);

        private ActivityModSet _Add<TTarget>(ModType modType, TTarget target, string targetProp, Dice dice, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
        {            var mod = new SyncedMod(Id, modType)
                .SetProps(target, PropName(targetProp), dice, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }





        public ActivityModSet Add<TOwner, TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TOwner : RpgObject
            where TSource : RpgObject
                => _Add<TOwner, TSource, TSourceValue>(ModType.Base, targetProp, source, sourceExpr, valueCalc);

        public ActivityModSet AddResult<TOwner, TSource, TSourceValue>(string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TOwner : RpgObject
            where TSource : RpgObject
                => _Add<TOwner, TSource, TSourceValue>(ModType.Override, targetProp, source, sourceExpr, valueCalc);

        private ActivityModSet _Add<TOwner, TSource, TSourceValue>(ModType modType, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TOwner : RpgObject
            where TSource : RpgObject
        {
            var owner = Graph!.GetObject<TOwner>(OwnerId)!;
            var mod = new SyncedMod(Id, modType)
                .SetProps(owner, PropName(targetProp), source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }

        public ActivityModSet Add<TTarget, TOwnerValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TOwnerValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
                => _Add(ModType.Base, target, targetExpr, source, sourceExpr, valueCalc);

        public ActivityModSet AddResult<TTarget, TOwnerValue, TSource, TSourceValue>(TTarget target, Expression<Func<TTarget, TOwnerValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
                => _Add(ModType.Override, target, targetExpr, source, sourceExpr, valueCalc);

        private ActivityModSet _Add<TTarget, TOwnerValue, TSource, TSourceValue>(ModType modType, TTarget target, Expression<Func<TTarget, TOwnerValue>> targetExpr, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? valueCalc = null)
            where TTarget : RpgObject
            where TSource : RpgObject
        {
            var mod = new SyncedMod(Id, modType)
                .SetProps(target, targetExpr, source, sourceExpr, valueCalc)
                .Create();

            AddMods(mod);

            return this;
        }


        private string PropName(string prop)
            => $"{Name}/{prop}";

        //private string PropName<TTarget, TResult>(this T modSet, Expression<Func<T, TResult>> expression)
        //{
        //    var memberExpression = expression.Body as MemberExpression;
        //    if (memberExpression == null)
        //        throw new ArgumentException($"Invalid path expression. {expression.Name} not a member expression");

        //    var prop = memberExpression.Member.Name;

        //    return $"{modSet.Name}_{prop}";
        //}

    }
}
