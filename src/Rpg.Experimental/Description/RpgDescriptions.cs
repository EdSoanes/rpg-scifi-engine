using System.Text;

namespace Rpg.Experimental.Description
{
    /// <summary>
    /// A short description of a calculation function, shown when a value that uses it is described.
    /// e.g. [Describe("Half of the score above ten, rounded down")]
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public sealed class DescribeAttribute : Attribute
    {
        public string Text { get; }

        public DescribeAttribute(string text)
            => Text = text;
    }

    /// <summary>
    /// Whether a mod is part of a property's value, and if not why not
    /// </summary>
    public enum RpgModStatus
    {
        /// <summary>The mod is part of the value</summary>
        Counts,
        /// <summary>The mod counts but what it comes from is waiting for a roll, so it adds nothing yet</summary>
        WaitingForRoll,
        Expired,
        NotStarted,
        /// <summary>Switched off by hand</summary>
        SwitchedOff,
        /// <summary>The state, mod set or action it belongs to is not applied</summary>
        NotApplied,
        /// <summary>Set aside by an override</summary>
        Overridden,
        /// <summary>Replaced by a newer version</summary>
        Replaced,
        /// <summary>A limit (minimum/maximum). Limits are not applied by the engine yet.</summary>
        Limit
    }

    /// <summary>
    /// Where a mod came from
    /// </summary>
    public enum RpgOriginKind
    {
        StartingValue,
        /// <summary>Derived from another property</summary>
        Derived,
        State,
        ModSet,
        ActionCost,
        ActionEffect,
        /// <summary>A value or bonus of an input of an action in progress</summary>
        ActionInput,
        /// <summary>A roll requested by rules code</summary>
        RollRequest,
        /// <summary>A change made by hand</summary>
        Manual,
        /// <summary>Added by rules code with nothing more known about it</summary>
        Rules
    }

    public enum RpgStateReason
    {
        ConditionMet,
        ConditionNotMet,
        /// <summary>Switched on for a time, e.g. by an action</summary>
        Activated,
        SwitchedOnByHand,
        SwitchedOffByHand,
        NotApplied,
        /// <summary>The object the state belongs to is not active</summary>
        OwnerNotActive
    }

    public sealed class RpgOriginDescription
    {
        public RpgOriginKind Kind { get; set; }

        /// <summary>The name of the state, mod set or action</summary>
        public string? Name { get; set; }

        /// <summary>The step of the action (Cost, Perform, Outcome)</summary>
        public string? Step { get; set; }

        /// <summary>The object the state, mod set or action belongs to</summary>
        public string? ObjectId { get; set; }
        public string? ObjectName { get; set; }

        /// <summary>The origin in words</summary>
        public string Text { get; set; } = string.Empty;

        public override string ToString()
            => Text;
    }

    public sealed class RpgCalculationDescription
    {
        /// <summary>The name of the calculation function</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>What the function does, from its Describe attribute</summary>
        public string? Description { get; set; }

        public Dice? Input { get; set; }
        public Dice? Output { get; set; }

        public string ToText()
            => $"{Description ?? Name} ({RpgDescriptionText.Value(Input)} gives {RpgDescriptionText.Value(Output)})";

        public override string ToString()
            => ToText();
    }

    public sealed class RpgModDescription
    {
        public string ModId { get; set; } = string.Empty;
        public string? Name { get; set; }

        /// <summary>Initial, Base, Override, Standard, Combine, Replace, Temporal...</summary>
        public string Kind { get; set; } = string.Empty;

        /// <summary>The property the mod changes</summary>
        public string TargetObjectId { get; set; } = string.Empty;
        public string TargetProp { get; set; } = string.Empty;

        /// <summary>What the mod adds. Null if it has nothing to add yet.</summary>
        public Dice? Contribution { get; set; }

        /// <summary>True if the mod is part of the value now</summary>
        public bool Counts { get; set; }
        public RpgModStatus Status { get; set; }

        /// <summary>The reason in words when the mod does not count</summary>
        public string? WhyNot { get; set; }

        /// <summary>When the mod starts and ends, in words</summary>
        public string Lifespan { get; set; } = string.Empty;

        public bool IsManual { get; set; }
        public RpgOriginDescription Origin { get; set; } = new();

        /// <summary>The value, when the mod's source is a fixed value</summary>
        public Dice? FixedValue { get; set; }

        /// <summary>The property the mod comes from, when there is one</summary>
        public string? SourceObjectId { get; set; }
        public string? SourceProp { get; set; }
        public RpgPropertyDescription? SourceProperty { get; set; }

        public RpgCalculationDescription? Calculation { get; set; }

        public string ToText(int indent = 0)
        {
            var sb = new StringBuilder();
            AppendTo(sb, indent);
            return sb.ToString().TrimEnd();
        }

        internal void AppendTo(StringBuilder sb, int indent)
        {
            var pad = new string(' ', indent * 2);

            //A source with no value of its own adds nothing, which reads better as +0
            sb.Append(pad)
                .Append(Status == RpgModStatus.WaitingForRoll ? "(nothing yet)" : RpgDescriptionText.Signed(Contribution ?? Dice.Zero))
                .Append(' ')
                .Append(Name ?? Kind)
                .Append(": ")
                .Append(Origin.Text);

            if (!string.IsNullOrEmpty(Lifespan) && Lifespan != RpgDescriptionText.Permanent)
                sb.Append(", ").Append(Lifespan);

            if (!Counts)
                sb.Append(" [not counted: ").Append(WhyNot).Append(']');
            else if (Status == RpgModStatus.WaitingForRoll)
                sb.Append(" [").Append(WhyNot).Append(']');

            sb.AppendLine();

            if (Calculation != null)
                sb.Append(pad).Append("    calculation: ").AppendLine(Calculation.ToText());

            SourceProperty?.AppendTo(sb, indent + 2);
        }

        public override string ToString()
            => $"{RpgDescriptionText.Signed(Contribution)} {Name ?? Kind}: {Origin.Text}";
    }

    /// <summary>
    /// Why a property has the value it has: every mod on it, and for each mod where it came from
    /// </summary>
    public sealed class RpgPropertyDescription
    {
        public string ObjectId { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public string? Archetype { get; set; }
        public string Prop { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>What the property is now</summary>
        public Dice Value { get; set; }

        /// <summary>What the mods add up to, before any roll</summary>
        public Dice? Expression { get; set; }

        /// <summary>The value without temporary changes</summary>
        public Dice? BaseValue { get; set; }

        public RpgRoll? Roll { get; set; }

        /// <summary>A number is wanted and the dice have not been rolled</summary>
        public bool IsRollPending { get; set; }

        /// <summary>There is a stored roll but it was for different dice</summary>
        public bool IsRollOutOfDate { get; set; }

        /// <summary>The property is already being described further up the tree. Its mods are not repeated.</summary>
        public bool IsLoop { get; set; }

        /// <summary>The tree was cut off here by the depth asked for. The property does have mods.</summary>
        public bool IsTruncated { get; set; }

        public List<RpgModDescription> Mods { get; set; } = new();

        public string ToText(int indent = 0)
        {
            var sb = new StringBuilder();
            AppendTo(sb, indent);
            return sb.ToString().TrimEnd();
        }

        internal void AppendTo(StringBuilder sb, int indent)
        {
            sb.Append(new string(' ', indent * 2))
                .Append(ObjectName).Append('.').Append(DisplayName)
                .Append(" = ").Append(Value.ToString());

            if (Roll != null && !IsRollOutOfDate && !IsRollPending && Expression != null && !Expression.Value.IsConstant)
                sb.Append($" ({Expression}: rolled {Roll.Result} on {Roll.DicePart} by the {Roll.SuppliedBy.ToString().ToLower()}{(Roll.IsOutOfRange ? ", out of range" : "")})");

            if (IsRollPending)
                sb.Append(IsRollOutOfDate
                    ? $" (roll pending, the earlier roll of {Roll!.Result} was on {Roll.DicePart})"
                    : " (roll pending)");

            if (IsLoop)
                sb.Append(" (already shown above)");

            if (IsTruncated)
                sb.Append(" (...)");

            sb.AppendLine();

            foreach (var mod in Mods)
                mod.AppendTo(sb, indent + 1);
        }

        public override string ToString()
            => $"{ObjectName}.{DisplayName} = {Value}";
    }

    /// <summary>
    /// What a mod set or state changes on one property
    /// </summary>
    public sealed class RpgChangeDescription
    {
        public string ObjectId { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public string Prop { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;

        /// <summary>The total of what the mods add to the property</summary>
        public Dice Change { get; set; }

        public List<RpgModDescription> Mods { get; set; } = new();

        public override string ToString()
            => $"{ObjectName}.{DisplayName} {RpgDescriptionText.Signed(Change)}";
    }

    /// <summary>
    /// What a mod set or a state does, and for a state why it is on or off
    /// </summary>
    public sealed class RpgModSetDescription
    {
        public string Id { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public bool IsApplied { get; set; }
        public string Lifespan { get; set; } = string.Empty;

        public bool IsState { get; set; }
        public bool IsOn { get; set; }
        public bool NeedsTurnTracking { get; set; }
        public RpgStateReason? Reason { get; set; }

        /// <summary>Why the state is on or off, in words</summary>
        public string? ReasonText { get; set; }

        /// <summary>What has switched the state on for a time (e.g. an action until turn 3)</summary>
        public List<RpgModDescription> Activations { get; set; } = new();

        public List<RpgChangeDescription> Changes { get; set; } = new();

        public string ToText()
        {
            var sb = new StringBuilder();

            sb.Append(IsState ? "State " : "Mod set ").Append(Name ?? Id);
            if (OwnerName != null)
                sb.Append(" on ").Append(OwnerName);

            if (IsState)
                sb.Append(": ").Append(IsOn ? "on" : "off").Append(". ").Append(ReasonText);
            else
                sb.Append(": ").Append(IsApplied ? "applied" : "not applied");

            if (NeedsTurnTracking)
                sb.Append(". Needs turns counted");

            sb.AppendLine();

            foreach (var change in Changes)
                sb.Append("  ").AppendLine(change.ToString());

            return sb.ToString().TrimEnd();
        }

        public override string ToString()
            => $"{(IsState ? "State" : "Mod set")} {Name ?? Id}";
    }

    /// <summary>
    /// One change an action makes, or would have made
    /// </summary>
    public sealed class RpgEffectDescription
    {
        public string ModId { get; set; } = string.Empty;
        public string? Name { get; set; }
        public string ObjectId { get; set; } = string.Empty;
        public string ObjectName { get; set; } = string.Empty;
        public string Prop { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Dice? Change { get; set; }
        public string Lifespan { get; set; } = string.Empty;

        /// <summary>Why it was skipped or dropped, when it was</summary>
        public string? Reason { get; set; }

        /// <summary>The change switches a state on for a time</summary>
        public bool IsStateActivation { get; set; }

        public override string ToString()
        {
            var text = IsStateActivation
                ? $"{ObjectName}: {DisplayName} on"
                : $"{ObjectName}.{DisplayName} {RpgDescriptionText.Signed(Change)}";

            if (Name != null)
                text += $" ({Name})";

            if (!string.IsNullOrEmpty(Lifespan))
                text += $", {Lifespan}";

            if (Reason != null)
                text += $" [{Reason}]";

            return text;
        }
    }

    public sealed class RpgInputDescription
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool IsNullable { get; set; }

        /// <summary>The value as text. Null if the input has no value yet.</summary>
        public string? Value { get; set; }

        /// <summary>The name of the object, when the input is an object</summary>
        public string? ObjectName { get; set; }

        public bool IsRollPending { get; set; }

        /// <summary>How the value is made up, when the input is a number or dice</summary>
        public RpgPropertyDescription? Property { get; set; }

        public override string ToString()
            => $"{Name} = {ObjectName ?? Value ?? (IsRollPending ? "(roll pending)" : "(not set)")}";
    }

    public sealed class RpgStepDescription
    {
        public string Name { get; set; } = string.Empty;
        public bool IsDone { get; set; }
        public List<RpgInputDescription> Inputs { get; set; } = new();

        public override string ToString()
            => $"{Name}{(IsDone ? " (done)" : "")}";
    }

    public sealed class RpgRollDescription
    {
        public string Prop { get; set; } = string.Empty;
        public Dice Expression { get; set; }
        public RpgRoll Roll { get; set; }

        public override string ToString()
            => $"{Prop}: {Expression}, rolled {Roll.Result} on {Roll.DicePart} by the {Roll.SuppliedBy.ToString().ToLower()}";
    }

    /// <summary>
    /// An action in progress or completed: its inputs, rolls, costs and effects
    /// </summary>
    public sealed class RpgActionDescription
    {
        public string ActivityActionId { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
        public string? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public bool IsPerformable { get; set; }
        public bool IsComplete { get; set; }

        public List<RpgStepDescription> Steps { get; set; } = new();
        public List<RpgPendingRoll> PendingRolls { get; set; } = new();
        public List<RpgRollDescription> Rolls { get; set; } = new();
        public List<RpgEffectDescription> Costs { get; set; } = new();
        public List<RpgEffectDescription> SkippedCosts { get; set; } = new();
        public List<RpgEffectDescription> Effects { get; set; } = new();
        public List<RpgEffectDescription> DroppedEffects { get; set; } = new();
        public List<RpgActionRef> FollowOnActions { get; set; } = new();

        public string ToText()
        {
            var sb = new StringBuilder();

            sb.Append(ActionName);
            if (OwnerName != null)
                sb.Append(" (").Append(OwnerName).Append(')');

            sb.AppendLine(IsComplete ? ": completed" : ": in progress");

            foreach (var step in Steps)
            {
                sb.Append("  ").AppendLine(step.ToString());
                foreach (var input in step.Inputs)
                    sb.Append("    ").AppendLine(input.ToString());
            }

            Section(sb, "Rolls pending", PendingRolls.Select(x => $"{x.Prop}: {x.Expression}"));
            Section(sb, "Rolls", Rolls.Select(x => x.ToString()));
            Section(sb, "Costs", Costs.Select(x => x.ToString()));
            Section(sb, "Costs skipped", SkippedCosts.Select(x => x.ToString()));
            Section(sb, "Effects", Effects.Select(x => x.ToString()));
            Section(sb, "Effects dropped", DroppedEffects.Select(x => x.ToString()));
            Section(sb, "Follow-on actions", FollowOnActions.Select(x => $"{x.ActionName}{(x.Optional ? " (optional)" : "")}"));

            return sb.ToString().TrimEnd();
        }

        private static void Section(StringBuilder sb, string title, IEnumerable<string> lines)
        {
            if (!lines.Any())
                return;

            sb.Append("  ").Append(title).AppendLine(":");
            foreach (var line in lines)
                sb.Append("    ").AppendLine(line);
        }

        public override string ToString()
            => $"{ActionName}{(IsComplete ? " (completed)" : "")}";
    }

    public sealed class RpgPropertySummary
    {
        public string Prop { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public Dice Value { get; set; }

        /// <summary>The value the property started with or derives, before overrides and other changes</summary>
        public Dice? OriginalValue { get; set; }

        /// <summary>The value differs from the original value</summary>
        public bool IsChanged { get; set; }
        public bool IsRollPending { get; set; }

        public override string ToString()
            => $"{DisplayName} = {Value}{(IsChanged ? $" (was {RpgDescriptionText.Value(OriginalValue)})" : "")}{(IsRollPending ? " (roll pending)" : "")}";
    }

    /// <summary>
    /// A summary of an object: each property and its value, and the states that are on. No trees.
    /// </summary>
    public sealed class RpgObjectDescription
    {
        public string ObjectId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Archetype { get; set; }
        public List<RpgPropertySummary> Properties { get; set; } = new();
        public List<string> StatesOn { get; set; } = new();
        public List<string> StatesOff { get; set; } = new();

        public string ToText()
        {
            var sb = new StringBuilder();
            sb.Append(Name).Append(" (").Append(Archetype).AppendLine(")");

            foreach (var prop in Properties)
                sb.Append("  ").AppendLine(prop.ToString());

            if (StatesOn.Any())
                sb.Append("  States on: ").AppendLine(string.Join(", ", StatesOn));

            return sb.ToString().TrimEnd();
        }

        public override string ToString()
            => $"{Name} ({Archetype})";
    }

    internal static class RpgDescriptionText
    {
        public const string Permanent = "permanent";

        public static string Value(Dice? dice)
            => dice?.ToString() ?? "nothing";

        /// <summary>
        /// A contribution with its sign: +2, -1, +2d6
        /// </summary>
        public static string Signed(Dice? dice)
        {
            if (dice == null)
                return "(nothing yet)";

            var text = dice.Value.ToString();
            return text.StartsWith('-') ? text : $"+{text}";
        }
    }
}
