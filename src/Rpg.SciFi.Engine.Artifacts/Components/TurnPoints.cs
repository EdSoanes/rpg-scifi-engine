using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Meta;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class TurnPoints : Modifiable
    {
        public TurnPoints() 
        {
            Name = nameof(TurnPoints);
        }

        [JsonProperty] public int BaseAction { get; protected set; }
        [JsonProperty] public int BaseExertion { get; protected set; }
        [JsonProperty] public int BaseFocus { get; protected set; }

        [Moddable] public int Action { get => this.Resolve(nameof(Action)); }
        [Moddable] public int Exertion { get => this.Resolve(nameof(Exertion)); }
        [Moddable] public int Focus { get => this.Resolve(nameof(Focus)); }
    }
}
