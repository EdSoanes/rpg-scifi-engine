using Rpg.ModObjects.Props;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using System.Reflection.Metadata.Ecma335;

namespace Rpg.ModObjects.Mods
{
    public class ModSubSet : ModSet
    {
        public string TargetId { get; set; }
        public string TargetProp { get; set; }
        public Dice Dice { get; private set; }
        public bool IsResolved { get => Dice.IsConstant; }

        public Mod Resolve(ModTemplate template, int? resolution = null)
        {
            if (IsResolved || resolution == null)
                resolution = Dice.Roll();

            var mod = template
                .SetProps(TargetId, Name, resolution.Value)
                .Create();

            return mod;
        }

        public ModSubSet(ILifecycle lifecycle, string initiatorId, PropRef recipientPropRef, Mod[] mods, Dice dice)
            : base(lifecycle, initiatorId, recipientPropRef.Prop)
        {
            TargetId = recipientPropRef.EntityId;
            TargetProp = recipientPropRef.Prop;
            Dice = dice;
            Mods.AddRange(mods);
        }

        public override string ToString()
        {
            return $"{TargetProp} ({Mods.Count()} mods)";
        }
    }
}
