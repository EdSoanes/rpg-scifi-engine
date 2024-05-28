using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.ModObjects
{
    public class ModSubSet : ModSet
    {
        public Guid RecipientId { get; set; }
        public Dice Dice { get; private set; }
        public bool IsResolved { get => Dice.IsConstant; }

        public Mod Resolve(ModBehavior behavior, int? resolution = null)
        {
            if (IsResolved || resolution == null)
                resolution = Dice.Roll();

            return Mod.Create(behavior, InitiatorId, Name, resolution.Value);
        }

        public ModSubSet(ModPropRef recipientPropRef, Mod[] mods, Dice dice)
            : base(recipientPropRef.EntityId, recipientPropRef.Prop, new Turn())
        {
            Dice = dice;
            Mods.AddRange(mods);
        }
    }
}
