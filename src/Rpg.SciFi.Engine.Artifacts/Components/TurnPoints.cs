using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class TurnPoints : Entity
    {
        [JsonConstructor] private TurnPoints() { }

        public TurnPoints(int baseAction, int baseExertion, int baseFocus)
        {
            BaseAction = baseAction;
            BaseExertion = baseExertion;
            BaseFocus = baseFocus;
        }

        [JsonProperty] public int BaseAction { get; protected set; }
        [JsonProperty] public int BaseExertion { get; protected set; }
        [JsonProperty] public int BaseFocus { get; protected set; }

        [Moddable] public int MaxAction { get => this.Resolve(nameof(MaxAction)); }
        [Moddable] public int MaxExertion { get => this.Resolve(nameof(MaxExertion)); }
        [Moddable] public int MaxFocus { get => this.Resolve(nameof(MaxFocus)); }

        [Moddable] public int Action { get => this.Resolve(nameof(Action)); }
        [Moddable] public int Exertion { get => this.Resolve(nameof(Exertion)); }
        [Moddable] public int Focus { get => this.Resolve(nameof(Focus)); }

        [Setup]
        public void Setup()
        {
            this.Mod(() => BaseAction, () => MaxAction).IsBase().Apply();
            this.Mod(() => BaseExertion, () => MaxExertion).IsBase().Apply();
            this.Mod(() => BaseFocus, () => MaxFocus).IsBase().Apply();

            this.Mod(() => MaxAction, () => Action).IsBase().Apply();
            this.Mod(() => MaxExertion, () => Exertion).IsBase().Apply();
            this.Mod(() => MaxFocus, () => Focus).IsBase().Apply();
        }
    }
}
