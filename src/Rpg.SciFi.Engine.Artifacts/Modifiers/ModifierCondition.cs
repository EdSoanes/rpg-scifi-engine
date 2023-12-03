using Newtonsoft.Json;

namespace Rpg.SciFi.Engine.Artifacts.Modifiers
{
    public enum ModifierConditionType
    {
        Base = -1,
        Instant = -2,
        State = -3
    }

    public class ModifierCondition
    {
        public string? State { get; set; }
        public int OnTurn { get; set; }

        [JsonConstructor] private ModifierCondition() { }
        public ModifierCondition(string state)
        {
            State = state;
            OnTurn = (int)ModifierConditionType.State;
        }

        public ModifierCondition(ModifierConditionType type)
        {
            OnTurn = (int)type;
        }

        public ModifierCondition(int onTurn)
        {
            OnTurn = onTurn;
        }
    }
}
