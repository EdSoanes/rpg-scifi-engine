using Newtonsoft.Json;

namespace Rpg.Sys.Moddable
{
    public class ModDuration
    {
        private const int WhenValueIsZero = -2;
        private const int AtEndOfTurn = -1;
        private const int AtEndOfEncounter = 0;
        private const int Never = int.MaxValue;

        [JsonProperty] public ModDurationType Type { get; private set; } = ModDurationType.Permanent;
        [JsonProperty] public int StartTurn { get; private set; } = int.MinValue;
        [JsonProperty] public int EndTurn { get; private set; } = Never;

        public bool CanRemove(int turn)
        {
            var expiry = GetExpiry(turn);
            return expiry != ModExpiry.Active && expiry != ModExpiry.Pending;
        }

        public void Expire(int turn) => SetOnTurnPeriod(turn - 1, turn - 1);

        public ModExpiry GetExpiry(int turn)
        {
            var expiry = ModExpiry.Active;

            if (EndTurn != WhenValueIsZero && EndTurn < int.MaxValue)
            {
                if (turn == 0 && Type == ModDurationType.EndOfTurn && EndTurn == 0)
                    expiry = ModExpiry.Active;
                else if (turn < 1 || StartTurn > EndTurn)
                    expiry = ModExpiry.Remove;
                else if (StartTurn > turn)
                    expiry = ModExpiry.Pending;
                else if (EndTurn < turn)
                    expiry = ModExpiry.Expired;
                else
                    expiry = ModExpiry.Active;
            }

            return expiry;
        }

        public void Set(ModDuration duration)
        {
            StartTurn = duration.StartTurn;
            EndTurn = duration.EndTurn;
            Type = duration.Type;
        }

        public void SetWhenPropertyZero()
        {
            Type = ModDurationType.WhenPropertyZero;
            EndTurn = WhenValueIsZero;
        }

        public void SetWhenEndOfTurn()
        {
            Type = ModDurationType.EndOfTurn;
            StartTurn = AtEndOfTurn;
            EndTurn = AtEndOfTurn;
        }

        public void SetWhenEndOfEncounter()
        {
            Type = ModDurationType.EndOfEncounter;
            EndTurn = AtEndOfEncounter;
        }
        
        public void SetTurn(int turn)
        {
            if (StartTurn == int.MinValue)
                StartTurn = turn;
            
            if (Type == ModDurationType.EndOfTurn || EndTurn == int.MaxValue)
                EndTurn = turn;
        }

        public void SetOnTurn(int turn)
        {
            Type = ModDurationType.OnTurn;
            EndTurn = turn;
        }

        public void SetOnTurnPeriod(int startTurn, int endTurn)
        {
            Type = ModDurationType.OnTurn;
            StartTurn = startTurn;
            EndTurn = endTurn;
        }

        public override string ToString()
        {
            var startTurn = TurnToString(StartTurn);
            var endTurn = TurnToString(EndTurn);

            return startTurn != endTurn
                ? $"{startTurn} => {endTurn}"
                : startTurn;
        }

        private string TurnToString(int turn)
        {
            var str = turn.ToString();

            if (turn == AtEndOfTurn)
                str = nameof(AtEndOfTurn);
            else if (turn == AtEndOfEncounter)
                str = nameof(AtEndOfEncounter);
            else if (turn == WhenValueIsZero)
                str = nameof(WhenValueIsZero);
            else
                str = turn.ToString();

            return str;
        }
    }
}
