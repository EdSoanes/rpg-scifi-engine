using Newtonsoft.Json;

namespace Rpg.ModObjects.Modifiers
{
    public class StateMod : Mod
    {
        [JsonConstructor] private StateMod() { }

        public StateMod(ModPropRef targetPropRef)
            : this(nameof(StateMod), targetPropRef)
        {
        }

        public StateMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Sum;
            Duration = ModDuration.OnValueZero();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class StateModExtensions
    {
        public static Mod AddStateMod<TEntity>(this TEntity entity, string targetProp)
            where TEntity : ModObject
        {
            var mod = Mod.Create<StateMod, TEntity>(targetProp, entity, targetProp, 1);
            entity.AddMod(mod);

            return mod;
        }
    }
}
