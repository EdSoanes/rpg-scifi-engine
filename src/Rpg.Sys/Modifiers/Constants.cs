namespace Rpg.Sys.Modifiers
{
    public enum PropReturnType
    {
        Integer,
        Dice
    }

    public enum PropType
    {
        Path,
        Dice
    }

    public enum ModifierDurationType
    {
        Permanent,
        OnTurn,
        EndOfTurn,
        EndOfEncounter,
        WhenPropertyZero
    }

    public static class RemoveTurn
    {

        public const int WhenZero = -2;
        public const int EndOfTurn = -1;
        public const int Encounter = 0;
    }

    public static class ModNames
    {
        public const string Base = "Base";
        public const string Damage = "Damage";
        public const string State = "State";
        public const string Cost = "Cost";
        public const string Turn = "Turn";
    }

    public enum ModifierExpiry
    {
        Active,
        Remove,
        Disabled,
        Pending,
        Expired
    }

    public enum ModifierType
    {
        Base,
        BaseOverride,
        Transient,
        State
    }

    public enum ModifierAction
    {
        Replace,
        Sum,
        Accumulate
    }
}
