using Rpg.SciFi.Engine.Artifacts.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Core
{
    public static class MetaDiscoveryExtensions
    {
        public static Modifier Mod<TTarget, T>(this TTarget obj, string modifierName, Expression<Func<TTarget, T>> targetExpr, Dice dice)
        {
            var target = MetaDiscovery.GetModLocator(obj, targetExpr);
            return new Modifier(modifierName, dice, target);
        }

        public static Modifier Modifies<TSource, TTarget, T>(this TSource source, TTarget target, string modifierName, Expression<Func<TSource, T>> sourceExpr, Expression<Func<TTarget, T>> targetExpr)
        {
            var sourceLocator = MetaDiscovery.GetModLocator(source, sourceExpr);
            var targetLocator = MetaDiscovery.GetModLocator(target, targetExpr);
            return new Modifier(modifierName, sourceLocator, targetLocator);
        }

        public static Modifier Modifies<TSource, T1, T2>(this TSource source, string modifierName, Expression<Func<TSource, T1>> sourceExpr, Expression<Func<TSource, T2>> targetExpr)
        {
            var sourceLocator = MetaDiscovery.GetModLocator(source, sourceExpr);
            var targetLocator = MetaDiscovery.GetModLocator(source, targetExpr);
            return new Modifier(modifierName, sourceLocator, targetLocator);
        }
    }
}
