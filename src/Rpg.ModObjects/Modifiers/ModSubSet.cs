using Rpg.ModObjects.Props;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Modifiers
{
    public class ModSubSet : ModSet
    {
        public Guid TargetId { get; set; }
        public Dice Dice { get; private set; }
        public bool IsResolved { get => Dice.IsConstant; }

        public Mod Resolve(ModBehavior behavior, int? resolution = null)
        {
            if (IsResolved || resolution == null)
                resolution = Dice.Roll();

            return Mod.Create(behavior, TargetId, Name, resolution.Value);
        }

        public ModSubSet(PropRef recipientPropRef, Mod[] mods, Dice dice)
            : base(recipientPropRef.EntityId, recipientPropRef.Prop, new Turn())
        {
            TargetId = recipientPropRef.EntityId;
            Dice = dice;
            Mods.AddRange(mods);
        }
    }
}
