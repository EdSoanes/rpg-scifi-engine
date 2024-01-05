using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Components
{
    public class TurnPoints : ModdableObject
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

        [Moddable] public int MaxAction { get => Resolve(); }
        [Moddable] public int MaxExertion { get => Resolve(); }
        [Moddable] public int MaxFocus { get => Resolve(); }

        [Moddable] public int Action { get => Resolve(); }
        [Moddable] public int Exertion { get => Resolve(); }
        [Moddable] public int Focus { get => Resolve(); }

        public override Modifier[] Setup()
        {
            return new[]
            {
                BaseModifier.Create(this, _baseAction, x => x.MaxAction),
                BaseModifier.Create(this, _baseExertion, x => x.MaxExertion),
                BaseModifier.Create(this, _baseFocus, x => x.MaxFocus),

                BaseModifier.Create(this, _baseAction, x => x.Action),
                BaseModifier.Create(this, _baseExertion, x => x.Exertion),
                BaseModifier.Create(this, _baseFocus, x => x.Focus)
            };
        }
    }
}
