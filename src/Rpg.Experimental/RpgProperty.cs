using Rpg.Experimental.System;

namespace Rpg.Experimental
{
    public class RpgProperty : MetaProperty
    {
        public string ObjectId { get; set; }
        public Dice? Value { get; set; }

        /// <summary>What the mods add up to, before any stored roll is used</summary>
        public Dice? Expression { get; set; }

        /// <summary>The stored roll of the dice in the expression</summary>
        public RpgRoll? Roll { get; set; }

        /// <summary>A number is wanted and the dice have not been rolled</summary>
        public bool IsRollPending { get; set; }
        public Dice? BaseValue { get; set; }
        public Dice? OriginalBaseValue { get; set; }
        public string[]? ChildObjectIds { get; set; }
    }
}
