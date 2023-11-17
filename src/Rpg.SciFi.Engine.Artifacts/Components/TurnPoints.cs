using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Attributes;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class TurnPoints : Modifiable
    {
        [JsonProperty] public int BaseAction { get; protected set; }
        [JsonProperty] public int BaseExertion { get; protected set; }
        [JsonProperty] public int BaseFocus { get; protected set; }

        [Modifiable("Action", "Action")] public int Action { get => BaseAction + ModifierRoll(nameof(Action)); }
        [Modifiable("Exertion", "Action")] public int Exertion { get => BaseExertion + ModifierRoll(nameof(Exertion)); }
        [Modifiable("Action", "Action")] public int Focus { get => BaseFocus + ModifierRoll(nameof(Focus)); }
    }
}
