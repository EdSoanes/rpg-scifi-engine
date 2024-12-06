namespace Rpg.ModObjects.Mods.Mods
{
    public class Turn : Time
    {
        public Turn()
            : base(nameof(Turn), 0, 1)
        { }

        public Turn(int duration)
            : base(nameof(Turn), 0, duration)
        { }

        public Turn(int startTurn, int duration)
            : base(nameof(Turn), startTurn, duration)
        { }

        public Turn(string ownerId)
            : base(ownerId, nameof(Turn), 0, 1)
        { }

        public Turn(string ownerId, int duration)
            : base(ownerId, nameof(Turn), 0, duration)
        { }

        public Turn(string ownerId, int startTurn, int duration)
            : base(ownerId, nameof(Turn), startTurn, duration)
        { }
    }
}
