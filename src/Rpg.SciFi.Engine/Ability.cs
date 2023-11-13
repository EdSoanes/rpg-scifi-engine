using Rpg.SciFi.Engine.Turns;
using System.Reflection;

namespace Rpg.SciFi.Engine
{
    public abstract class Ability
    {
        public const int PassiveAbility = -1;
        public const int FreeAbility = 0;

        private IEffectTarget[] _effectTargets;

        public virtual string Name { get; set; } = string.Empty;
        public virtual string Description { get; set; } = string.Empty;
        public virtual int ActionPointCost { get; set; } = FreeAbility;
        public virtual int Exertion { get; set; } = 0;

        public void OnUsed()
        {
            foreach (var effectTarget in _effectTargets)
                effectTarget.OnAbilityUsed(this.Name);
        }

        public Ability()
        {
            _effectTargets = new IEffectTarget[0];
        }

        public Ability(IEffectTarget effectTarget)
        {
            _effectTargets = new IEffectTarget[] { effectTarget };
        }

        public Ability(IEffectTarget[] effectTargets)
        {
            _effectTargets = effectTargets;
        }
    }
}
