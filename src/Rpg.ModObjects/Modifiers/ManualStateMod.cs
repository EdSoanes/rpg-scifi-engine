using Newtonsoft.Json;

namespace Rpg.ModObjects.Modifiers
{
    public class ManualStateMod : Mod
    {
        [JsonConstructor] private ManualStateMod() { }

        public ManualStateMod(ModPropRef targetPropRef)
            : this(nameof(ManualStateMod), targetPropRef)
        {
        }

        public ManualStateMod(string name, ModPropRef targetPropRef)
        {
            Name = name;
            ModifierType = ModType.Permanent;
            ModifierAction = ModAction.Replace;
            Duration = ModDuration.External();
            EntityId = targetPropRef.EntityId;
            Prop = targetPropRef.Prop;
        }
    }

    public static class ManualStateModExtensions
    {
        public static Mod AddManualStateMod<TEntity>(this TEntity entity, string state)
            where TEntity : ModObject
        {
            var mod = Mod.Create<ManualStateMod, TEntity>(state, entity, state, 1);
            entity.AddMod(mod);

            return mod;
        }

        public static void RemoveManualStateMod<TEntity>(this TEntity entity, string state)
            where TEntity : ModObject
        {
            entity.RemoveMods(state, ModType.Permanent);
        }
    }
}
