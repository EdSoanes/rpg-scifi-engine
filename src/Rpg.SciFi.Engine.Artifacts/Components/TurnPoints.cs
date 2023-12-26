using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class TurnPoints : Entity
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [JsonConstructor] public TurnPoints() { }

        public TurnPoints(int baseAction, int baseExertion, int baseFocus)
        {
            _baseAction = baseAction;
            _baseExertion = baseExertion;
            _baseFocus = baseFocus;
        }

        [Moddable] public int BaseAction { get => this.Resolve(nameof(BaseAction)); }
        [Moddable] public int BaseExertion { get => this.Resolve(nameof(BaseExertion)); }
        [Moddable] public int BaseFocus { get => this.Resolve(nameof(BaseFocus)); }

        [Moddable] public int MaxAction { get => this.Resolve(nameof(MaxAction)); }
        [Moddable] public int MaxExertion { get => this.Resolve(nameof(MaxExertion)); }
        [Moddable] public int MaxFocus { get => this.Resolve(nameof(MaxFocus)); }

        [Moddable] public int Action { get => this.Resolve(nameof(Action)); }
        [Moddable] public int Exertion { get => this.Resolve(nameof(Exertion)); }
        [Moddable] public int Focus { get => this.Resolve(nameof(Focus)); }

        [Setup]
        public Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseAction, x => x.BaseAction),
                BaseModifier.Create(this, _baseExertion, x => x.BaseExertion),
                BaseModifier.Create(this, _baseFocus, x => x.BaseFocus),

                BaseModifier.Create(this, x => x.BaseAction, x => x.MaxAction),
                BaseModifier.Create(this, x => x.BaseExertion, x => x.MaxExertion),
                BaseModifier.Create(this, x => x.BaseFocus, x => x.MaxFocus),

                BaseModifier.Create(this, x => x.BaseAction, x => x.Action),
                BaseModifier.Create(this, x => x.BaseExertion, x => x.Exertion),
                BaseModifier.Create(this, x => x.BaseFocus, x => x.Focus),
            };
        }
    }
}
