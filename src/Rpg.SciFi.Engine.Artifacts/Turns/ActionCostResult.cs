using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.MetaData;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.Turns
{
    public class ActionCostResult : Entity
    {
        private readonly int _baseAction;
        private readonly int _baseExertion;
        private readonly int _baseFocus;

        [Moddable] public int BaseActionCost { get => this.Resolve(nameof(BaseActionCost)); }
        [Moddable] public int BaseExertionCost { get => this.Resolve(nameof(BaseExertionCost)); }
        [Moddable] public int BaseFocusCost { get => this.Resolve(nameof(BaseFocusCost)); }

        [Moddable] public int ActionCost { get => this.Resolve(nameof(ActionCost)); }
        [Moddable] public int ExertionCost { get => this.Resolve(nameof(ExertionCost)); }
        [Moddable] public int FocusCost { get => this.Resolve(nameof(FocusCost)); }

        public ActionCostResult(string name, int actionCost, int exertionCost, int focusCost)
        {
            Name = name;

            _baseAction = actionCost;
            _baseExertion = exertionCost;
            _baseFocus = focusCost;
        }

        public bool ShouldResolve(int target = 0, int result = 0) => true;

        public void Resolve(IEntityManager context, Actor actor)
        {
            if (ActionCost != 0)
                context.AddMod(CostModifier.Create(ActionCost, actor, x => x.Turns.Action, () => Rules.Minus));

            if (ExertionCost != 0)
                context.AddMod(CostModifier.Create(ExertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

            if (FocusCost != 0)
                context.AddMod(CostModifier.Create(FocusCost, actor, x => x.Turns.Focus, () => Rules.Minus));
        }

        public List<Action> Actions { get; private set; } = new List<Action>();
    }
}
