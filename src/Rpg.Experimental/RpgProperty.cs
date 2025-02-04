using Rpg.Experimental.System;

namespace Rpg.Experimental
{
    public class RpgProperty : MetaProperty
    {
        public string ObjectId { get; set; }
        public Dice? Value { get; set; }
        public Dice? BaseValue { get; set; }
        public Dice? OriginalBaseValue { get; set; }
        public string[]? ChildObjectIds { get; set; }
    }
}
