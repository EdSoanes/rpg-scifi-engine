using Rpg.Experimental.Graph;

namespace Rpg.Experimental
{
    public enum RpgPropertyType
    {
        Int,
        Dice,
        Child,
        Children,
        Text
    }

    public enum LifecycleExpiry
    {
        Unset,
        Pending,
        Active,
        Suspended,
        Expired,
        Destroyed
    }

    public enum TimePointType
    {
        BeforeTime,
        TimeBegins,
        Waiting,
        EncounterBegins,
        Turn,
        EncounterEnds,
        TimePasses,
        TimeEnds
    }

    public enum StateInstanceType
    {
        Manual,
        Conditional,
        Timed,
    }

    public class ActionMethodNames
    {
        public const string CanPerform = "CanPerform";
        public const string Cost = "Cost";
        public const string Perform = "Perform";
        public const string Outcome = "Outcome";
    }

    public class ActionReservedArgs
    {
        public const string Owner = "owner";
        public const string Actor = "actor";
        public const string Action = "action";
        public const string Activity = "activity";
        public const string ActivityAction = "activityAction";
        public const string Context = "context";
        public const string CharacterSheet = "characterSheet";
        public const string Graph = "graph";

        public Type Type(string argName)
            => argName switch
            {
                Owner => typeof(RpgObject),
                Actor => typeof(RpgObject),
                Action => typeof(RpgAction),
                Activity => typeof(RpgActivity),
                ActivityAction => typeof(RpgActivityAction),
                CharacterSheet => typeof(RpgCharacterSheet),
                Graph => typeof(RpgGraph),
                _ => typeof(Dice)
            };
    }
}
