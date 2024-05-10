using Newtonsoft.Json;

namespace Rpg.ModObjects
{
    public class ModDuration
    {
        [JsonProperty] public ModDurationType Type { get; private set; } = ModDurationType.Permanent;
        [JsonProperty] public int StartTurn { get; private set; } = int.MinValue;
        [JsonProperty] public int EndTurn { get; private set; } = int.MaxValue;

        public bool CanRemove(int turn)
        {
            var expiry = GetExpiry(turn);
            return expiry == ModExpiry.Expired && turn <= 0;
        }

        public void Expire(int turn)
        {
            StartTurn = turn - 1;
            EndTurn = turn - 1;
        }

        public ModExpiry GetExpiry(int turn)
        {
            if ((Type == ModDurationType.OnNewTurn || Type == ModDurationType.EndOfEncounter || Type == ModDurationType.Timed) && turn == 0)
                return ModExpiry.Expired;

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

        public static ModDuration OnValueZero()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.OnValueZero
            };

            return duration;
        }

        public static ModDuration OnEncounterEnd()
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.EndOfEncounter
            };

            return duration;
        }

        public static ModDuration OnNewTurn(int turn)
        {
            var duration = new ModDuration
            {
                Type = ModDurationType.OnNewTurn,
                StartTurn = turn,
                EndTurn = turn
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
