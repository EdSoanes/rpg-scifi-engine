using Newtonsoft.Json;

namespace Rpg.Experimental
{
    public enum RpgRollSource
    {
        /// <summary>The app rolled the dice</summary>
        App,
        /// <summary>The player rolled real dice and supplied the result</summary>
        Player
    }

    /// <summary>
    /// Who rolls when a roll is needed and nobody has said
    /// </summary>
    public enum RpgRollMode
    {
        /// <summary>A step with a pending roll does not run. The roll is reported as pending.</summary>
        Ask,
        /// <summary>Running a step lets the app roll whatever the step still needs.</summary>
        App
    }

    /// <summary>
    /// The stored result of rolling the dice in a property's value. Only the dice are stored, not the
    /// bonuses, so the dice stay as rolled when a bonus changes.
    /// </summary>
    public sealed class RpgRoll
    {
        /// <summary>The dice that were rolled, e.g. 2d6</summary>
        [JsonProperty] public string DicePart { get; private set; }

        /// <summary>The total of the dice</summary>
        [JsonProperty] public int Result { get; private set; }

        /// <summary>Each die, when the app rolled. Dice that are subtracted are negative.</summary>
        [JsonProperty] public int[]? Dice { get; private set; }

        [JsonProperty] public RpgRollSource SuppliedBy { get; private set; }

        /// <summary>The result is not possible on those dice. It is used anyway.</summary>
        [JsonProperty] public bool IsOutOfRange { get; private set; }

        [JsonConstructor] private RpgRoll() { }

        internal RpgRoll(Dice dicePart, int result, int[]? dice, RpgRollSource suppliedBy)
        {
            DicePart = dicePart.ToString();
            Result = result;
            Dice = dice;
            SuppliedBy = suppliedBy;
            IsOutOfRange = result < dicePart.Min() || result > dicePart.Max();
        }

        /// <summary>
        /// True if this roll was made for the dice in the expression
        /// </summary>
        public bool AppliesTo(Dice? expression)
            => expression != null
                && !expression.Value.IsConstant
                && expression.Value.DicePart.ToString() == DicePart;

        public override string ToString()
            => $"{DicePart} = {Result} ({SuppliedBy})";
    }

    /// <summary>
    /// A roll that is needed and has not been settled
    /// </summary>
    public sealed class RpgPendingRoll
    {
        public string ObjectId { get; init; }
        public string Prop { get; init; }

        /// <summary>The whole expression, e.g. 2d6 + 1</summary>
        public Dice Expression { get; init; }

        /// <summary>The dice to roll, e.g. 2d6</summary>
        public Dice DicePart { get; init; }

        /// <summary>The action that needs the roll, if it is an input of an action</summary>
        public string? ActionName { get; init; }

        /// <summary>The steps of the action that need the roll</summary>
        public string[] Steps { get; init; } = [];

        /// <summary>An earlier roll that no longer applies because the dice have changed</summary>
        public RpgRoll? OutOfDateRoll { get; init; }

        public override string ToString()
            => $"{ObjectId}.{Prop}: {Expression}";
    }
}
