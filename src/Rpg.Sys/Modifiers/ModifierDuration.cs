using Newtonsoft.Json;

namespace Rpg.Sys.Modifiers
{
    public class ModifierDuration
    {
        private const int WhenZero = -2;
        private const int EndOfTurn = -1;
        private const int Encounter = 0;
        private const int Permanent = int.MaxValue;

        [JsonProperty] public ModifierDurationType Type { get; private set; } = ModifierDurationType.Permanent;
        [JsonProperty] public int StartTurn { get; private set; } = int.MinValue;
        [JsonProperty] public int EndTurn { get; private set; } = Permanent;

        public bool CanClear(int turn) => GetExpiry(turn) != ModifierExpiry.Active;

        public void Expire(int turn) => SetOnTurnPeriod(turn - 1, turn - 1);

        public ModifierExpiry GetExpiry(int turn)
        {
            var expiry = ModifierExpiry.Active;

            if (EndTurn != WhenZero && EndTurn < int.MaxValue)
            {
                if (turn == 0 && Type == ModifierDurationType.EndOfTurn && EndTurn == 0)
                    expiry = ModifierExpiry.Active;
                else if (turn < 1 || StartTurn > EndTurn)
                    expiry = ModifierExpiry.Remove;
                else if (StartTurn > turn)
                    expiry = ModifierExpiry.Pending;
                else if (EndTurn < turn)
                    expiry = ModifierExpiry.Expired;
                else
                    expiry = ModifierExpiry.Active;
            }

            return expiry;
        }

        public void SetWhenPropertyZero()
        {
            Type = ModifierDurationType.WhenPropertyZero;
            EndTurn = WhenZero;
        }

        public void SetWhenEndOfTurn()
        {
            Type = ModifierDurationType.EndOfTurn;
            StartTurn = EndOfTurn;
            EndTurn = EndOfTurn;
        }

        public void SetWhenEndOfEncounter()
        {
            Type = ModifierDurationType.EndOfEncounter;
            EndTurn = Encounter;
        }
        
        public void SetTurn(int turn)
        {
            if (StartTurn == int.MinValue)
                StartTurn = turn;
            
            if (Type == ModifierDurationType.EndOfTurn || EndTurn == int.MaxValue)
                EndTurn = turn;
        }

        public void SetOnTurn(int turn)
        {
            Type = ModifierDurationType.OnTurn;
            EndTurn = turn;
        }

        public void SetOnTurnPeriod(int startTurn, int endTurn)
        {
            Type = ModifierDurationType.OnTurn;
            StartTurn = startTurn;
            EndTurn = endTurn;
        }
    }
}
