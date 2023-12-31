using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine.Artifacts.MetaData
{
    public class TurnManager
    {
        private ModStore? _modStore;
        private EntityStore? _entityStore;
        
        [JsonProperty] public List<Turns.Action> Actions { get; private set; } = new List<Turns.Action>();

        public void Initialize(ModStore modStore, EntityStore entityStore)
        {
            _modStore = modStore;
            _entityStore = entityStore;
        }

        public int Current { get; private set; }

        public void StartEncounter() => Current = 1;
        public void Increment() => Current++;
        public void SetTurn(int turn) => Current = turn;
        public void EndEncounter() => Current = 0;

        public Turns.Action CreateAction(string name, int actionCost, int exertionCost, int focusCost)
        {
            var action = new Turns.Action(name, actionCost, exertionCost, focusCost);
            _entityStore!.Add(action);

            return action;
        }

        public Turns.Action? Apply(Character actor, Turns.Action turnAction, int diceRoll = 0)
        {
            var actionCost = turnAction.ActionCost;
            if (actionCost != 0)
                _modStore!.Add(CostModifier.Create(actionCost, actor, x => x.Turns.Action, () => Rules.Minus));

            var exertionCost = turnAction.ExertionCost;
            if (exertionCost != 0)
                _modStore!.Add(CostModifier.Create(exertionCost, actor, x => x.Turns.Exertion, () => Rules.Minus));

            var focusCost = turnAction.FocusCost;
            if (focusCost != 0)
                _modStore!.Add(CostModifier.Create(focusCost, actor, x => x.Turns.Focus, () => Rules.Minus));

            var modifiers = turnAction.Resolve(diceRoll);
            _modStore!.Add(modifiers);

            return turnAction.NextAction();
        }
    }
}
