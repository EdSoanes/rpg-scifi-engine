using Rpg.Experimental.Description;

namespace Rpg.Experimental.Server
{
    /// <summary>
    /// A request carries the whole character sheet, as the text given by RpgCharacterSheet.Save(), and one
    /// operation to perform on it. Nothing is kept between requests.
    /// </summary>
    public class RpgRequest<T>
    {
        public string Sheet { get; init; } = string.Empty;
        public T Op { get; init; } = default!;
    }

    /// <summary>
    /// A response carries the character sheet as it is after the operation, and what the operation returns
    /// </summary>
    public class RpgResponse<T>
    {
        public string Sheet { get; init; } = string.Empty;
        public T Data { get; init; } = default!;
    }

    /// <summary>
    /// Something that can be turned into a character sheet, e.g. a character authored in a content
    /// management system
    /// </summary>
    public class RpgContent
    {
        public Guid Key { get; set; }
        public string System { get; set; } = string.Empty;
        public string Archetype { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    /// <summary>
    /// Where authored objects come from. A host (e.g. a content management system) supplies this.
    /// </summary>
    public interface IContentFactory
    {
        RpgContent[] ListEntities(string systemIdentifier);
        RpgObject CreateEntity(string systemIdentifier, string archetype, string contentId);
    }

    /// <summary>
    /// The request could not be carried out because it refers to something that does not exist
    /// </summary>
    public class RpgServerException : Exception
    {
        public RpgServerException(string message)
            : base(message) { }
    }

    #region Views

    public class RpgTimeView
    {
        public bool IsTurnTracking { get; set; }
        public int Turn { get; set; }
        public string? LastEvent { get; set; }

        /// <summary>The turns that can be gone back to</summary>
        public int[] RewindableTurns { get; set; } = [];

        /// <summary>The time events of the game system, plus the built in "TimePasses"</summary>
        public string[] TimeEvents { get; set; } = [];

        /// <summary>True if something on the sheet needs turns to be counted</summary>
        public bool TurnTrackingNeeded { get; set; }

        /// <summary>What needs turns counted, in words</summary>
        public string[] TurnTrackingNeededFor { get; set; } = [];
    }

    public class RpgActionView
    {
        public string Name { get; set; } = string.Empty;
        public string OwnerId { get; set; } = string.Empty;
        public bool IsPerformable { get; set; }
    }

    public class RpgObjectView
    {
        public RpgObjectDescription Description { get; set; } = new();
        public List<RpgActionView> Actions { get; set; } = new();

        /// <summary>The ids of the objects in each child property, e.g. Hands</summary>
        public Dictionary<string, string[]> Children { get; set; } = new();
    }

    public class RpgManualChangeView
    {
        public string ModId { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public string Prop { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public Dice? Value { get; set; }
    }

    /// <summary>
    /// Everything a character sheet user interface needs to draw the sheet
    /// </summary>
    public class RpgSheetView
    {
        public string System { get; set; } = string.Empty;
        public string ActorId { get; set; } = string.Empty;
        public RpgTimeView Time { get; set; } = new();
        public RpgRollMode RollMode { get; set; }
        public int MaxTurnHistory { get; set; }
        public List<RpgObjectView> Objects { get; set; } = new();
        public List<RpgPendingRoll> PendingRolls { get; set; } = new();
        public List<RpgManualChangeView> ManualChanges { get; set; } = new();

        /// <summary>The actions that have been started and not completed</summary>
        public List<RpgActionDescription> ActionsInProgress { get; set; } = new();
    }

    public class RpgActionResult
    {
        /// <summary>
        /// False if the step did not run, e.g. because an input is missing or a roll is pending. The
        /// description says which.
        /// </summary>
        public bool Ran { get; set; }
        public RpgActionDescription Action { get; set; } = new();

        /// <summary>The actions nominated to follow, once the action is completed</summary>
        public RpgActionRef[] FollowOnActions { get; set; } = [];
    }

    #endregion Views

    #region Ops

    public class DescribeProp
    {
        public string ObjectId { get; set; } = string.Empty;
        public string Prop { get; set; } = string.Empty;

        /// <summary>How many levels of sources to follow. Leave empty for the whole tree.</summary>
        public int? Depth { get; set; }
    }

    public class DescribeState
    {
        public string ObjectId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }

    public class DescribeModSet
    {
        public string ModSetId { get; set; } = string.Empty;
    }

    public class DescribeObject
    {
        public string ObjectId { get; set; } = string.Empty;
    }

    public class DescribeAction
    {
        public string ActivityActionId { get; set; } = string.Empty;
    }

    public enum ChangeByHandKind
    {
        /// <summary>Set the value, overruling the rules</summary>
        Override,
        /// <summary>Add to (or take from) the value</summary>
        Adjust
    }

    public class ChangeByHand
    {
        public string ObjectId { get; set; } = string.Empty;
        public string Prop { get; set; } = string.Empty;

        /// <summary>A number or a dice expression</summary>
        public string Value { get; set; } = string.Empty;
        public ChangeByHandKind Kind { get; set; }
    }

    public class UndoChangeByHand
    {
        public string ModId { get; set; } = string.Empty;
    }

    public enum SetStateMode
    {
        /// <summary>Switch the state on by hand</summary>
        On,
        /// <summary>Switch the state off by hand</summary>
        Off,
        /// <summary>Let the rules decide again</summary>
        Auto
    }

    public class SetState
    {
        public string ObjectId { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public SetStateMode Mode { get; set; }
    }

    public enum TimeOpKind
    {
        BeginTurnTracking,
        NextTurn,
        AdvanceToTurn,
        RenumberTurn,
        EndTurnTracking,
        TimeEvent,
        RewindToTurn
    }

    public class TimeOp
    {
        public TimeOpKind Kind { get; set; }

        /// <summary>For BeginTurnTracking (optional), AdvanceToTurn, RenumberTurn and RewindToTurn</summary>
        public int? Turn { get; set; }

        /// <summary>For TimeEvent: the name of the event. Leave empty for "TimePasses".</summary>
        public string? Event { get; set; }
    }

    public class InitiateAction
    {
        /// <summary>Who is doing it</summary>
        public string InitiatorId { get; set; } = string.Empty;

        /// <summary>What the action belongs to, e.g. the weapon for an attack</summary>
        public string ActionOwnerId { get; set; } = string.Empty;
        public string ActionName { get; set; } = string.Empty;
    }

    public class ActionStepRun
    {
        public string ActivityActionId { get; set; } = string.Empty;

        /// <summary>Cost, Perform or Outcome</summary>
        public string Step { get; set; } = string.Empty;

        /// <summary>
        /// Values for the inputs of the step, by name. A number given for an input that has dice in it is
        /// the result of rolling those dice.
        /// </summary>
        public Dictionary<string, object?> Args { get; set; } = new();
    }

    public class ActionComplete
    {
        public string ActivityActionId { get; set; } = string.Empty;
    }

    public class ActionAutoComplete
    {
        public string ActivityActionId { get; set; } = string.Empty;
        public Dictionary<string, object?> Args { get; set; } = new();
    }

    public class ActionReset
    {
        public string ActivityActionId { get; set; } = string.Empty;

        /// <summary>The step to do again (and the steps after it). Leave empty to undo the whole action.</summary>
        public string? Step { get; set; }
    }

    public enum RollOpKind
    {
        /// <summary>The app rolls the dice of one property</summary>
        Roll,
        /// <summary>Store a result the player rolled</summary>
        Set,
        /// <summary>Remove a stored roll</summary>
        Clear,
        /// <summary>The app rolls everything that is pending</summary>
        RollAllPending
    }

    public class RollOp
    {
        public RollOpKind Kind { get; set; }
        public string? ObjectId { get; set; }
        public string? Prop { get; set; }

        /// <summary>For Set: the total of the dice, without bonuses</summary>
        public int? Result { get; set; }
    }

    public class SheetSettings
    {
        public RpgRollMode? RollMode { get; set; }
        public int? MaxTurnHistory { get; set; }
    }

    #endregion Ops
}
