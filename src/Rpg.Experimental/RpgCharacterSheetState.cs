using Rpg.Experimental.Graph;

namespace Rpg.Experimental
{
    public class RpgCharacterSheetState : RpgGraphState
    {
        public string ActorId { get; init; }
        public Dictionary<int, RpgTurnSnapshot> TurnSnapshots { get; set; } = new();
    }
}
