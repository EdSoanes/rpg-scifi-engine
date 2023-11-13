using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rpg.SciFi.Engine
{
    public abstract class Artifact : IEffectTarget
    {
        public string Name { get; internal set; } = string.Empty;
        public string Description { get; internal set; } = string.Empty;
        public double Weight { get; internal set; } = 0.0;

        #region States

        public State[] States { get; internal set; } = new State[0];
        public List<State> ActiveStates { get; internal set; } = new List<State>();

        public bool HasState(string stateName)
        {
            return States.Any(x => x.Name == stateName);
        }

        public void ActivateState(string stateName)
        {
            var state = States.FirstOrDefault(x => x.Name == stateName);
            if (state != null && !ActiveStates.Contains(state))
                ActiveStates.Add(state);
        }

        public void DeactivateState(string stateName)
        {
            var states = ActiveStates.Where(x => x.Name == stateName);
            foreach (var state in states)
                ActiveStates.Remove(state);
        }

        #endregion States

        public Ability[] Abilities { get; set; } = new Ability[0];
        public List<Condition> Conditions { get; set; } = new List<Condition>();

        public EmissionSignature Emissions { get; internal set; } = new EmissionSignature();
        public ResistanceSignature Resistances { get; internal set; } = new ResistanceSignature();

        public Artifact[] Accessories { get; set; } = new Artifact[0];



        public void OnAbilityUsed(string abilityName)
        {
            var abilityUsed = Abilities.FirstOrDefault(x => x.Name == abilityName);
            if (abilityUsed != null)
            {
                OnAbilityUsed(abilityUsed);
                foreach (var accessory in Accessories)
                    accessory.OnAbilityUsed(abilityName);
            }
        }

        protected virtual void OnAbilityUsed(Ability ability)
        {

        }
    }
}
