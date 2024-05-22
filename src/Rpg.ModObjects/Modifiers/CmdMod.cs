using Rpg.ModObjects.Values;
using System.Linq.Expressions;
using System.Text.Json.Serialization;

namespace Rpg.ModObjects.Modifiers
{
    public class CmdMod : Mod
    {
        [JsonConstructor] private CmdMod() { }

        public CmdMod(ModPropRef targetPropRef)
            : this(nameof(CmdMod), targetPropRef)
        {
        }

        public CmdMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.External();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class CmdModExtensions
    {
        public static ModSet<TTarget> Add<TTarget, TTargetValue>(this ModSet<TTarget> modSet, string name, TTarget entity, Expression<Func<TTarget, TTargetValue>> targetExpr, Dice dice, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<CmdMod, TTarget, TTargetValue>(name, entity, targetExpr, dice, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }

        public static ModSet<TTarget> Add<TTarget, TSource, TSourceValue>(this ModSet<TTarget> modSet, string name, TTarget target, string targetProp, TSource source, Expression<Func<TSource, TSourceValue>> sourceExpr, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
            where TSource : ModObject
        {
            var mod = Mod.Create<CmdMod, TTarget, TSource, TSourceValue>(name, target, targetProp, source, sourceExpr, diceCalcExpr);
            modSet.Add(mod);

            return modSet;
        }
    }
}
