using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModDuration
    {
        internal const int PermanentStart = int.MinValue;
        internal const int PermanentEnd = int.MaxValue;
        internal const int EndEncounter = int.MaxValue - 1;
        internal const int BeginEncounter = 0;
        internal const int Expired = -1;

        [JsonProperty] public ModDurationType Type { get; private set; } = ModDurationType.Permanent;
        [JsonProperty] public int StartTurn { get; private set; } = PermanentStart;
        [JsonProperty] public int EndTurn { get; private set; } = PermanentEnd;

        public bool CanRemove(int turn)
        {
            var expiry = GetExpiry(turn);
            return expiry == ModExpiry.Expired && (turn == EndEncounter || turn == BeginEncounter);
        }

        public void SetDuration(int startTurn, int endTurn)
        {
            StartTurn = startTurn;
            EndTurn = endTurn;
        }

        public void SetExpired()
        {
            StartTurn = Expired;
            EndTurn = Expired;
        }

        public void SetActive()
        {
            StartTurn = PermanentStart;
            EndTurn = PermanentEnd;
        }

        public void SetPending(int turn)
        {
            StartTurn = turn + 1;
            EndTurn = PermanentEnd;
        }

        public ModExpiry GetExpiry(int turn)
        {
            if (EndTurn < turn)
                return ModExpiry.Expired;

            if (StartTurn > turn)
                return ModExpiry.Pending;

            return ModExpiry.Active;
        }

        public static ModDuration Permanent()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.Permanent
            };

            return duration;
        }

        public static ModDuration External()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.External
            };

            return duration;
        }

        public static ModDuration Timed(int startTurn, int endTurn)
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.Timed,
                StartTurn = startTurn,
                EndTurn = endTurn
            };

            return duration;
        }

        public static ModDuration OnNewTurn(int turn)
            => Timed(turn, turn);

        public static ModDuration OnValueZero()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.OnValueZero
            };

            return duration;
        }

        public static ModDuration OnEndEncounter()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.Timed,
                EndTurn = EndEncounter - 1,
            };

            return duration;
        }

        public static ModDuration OnBeginEncounter()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.Timed,
                StartTurn = BeginEncounter,
            };

            return duration;
        }

        public override string ToString()
        {
            return Type == ModDurationType.Timed
                ? $"{StartTurn} => {EndTurn}"
                : $"{Type}";
        }
    }
}
