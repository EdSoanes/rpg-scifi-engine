using Newtonsoft.Json;
using Rpg.ModObjects.Values;
using System.Linq.Expressions;

namespace Rpg.ModObjects.Modifiers
{
    internal class BaseInitMod : Mod
    {
        [JsonConstructor] private BaseInitMod() { }

        public BaseInitMod(ModPropRef targetPropRef)
            : this(nameof(BaseInitMod), targetPropRef)
        {
        }

        public BaseInitMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.BaseInit;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.Permanent();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class BaseInitModExtensions
    {
        public static Mod AddBaseInitMod<TTarget>(this TTarget entity, string prop, Dice value, Expression<Func<Func<Dice, Dice>>>? diceCalcExpr = null)
            where TTarget : ModObject
        {
            var mod = Mod.Create<BaseInitMod, TTarget>(prop, entity, prop, value, diceCalcExpr);
            entity.AddMod(mod);
            return mod;
        }
    }
}
