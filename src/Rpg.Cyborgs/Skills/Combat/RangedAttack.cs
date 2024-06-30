using Newtonsoft.Json;
using Rpg.Cyborgs.States;
using Rpg.ModObjects;
using Rpg.ModObjects.Lifecycles;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Skills.Combat
{
    public class RangedAttack : Skill
    {
        [JsonConstructor] private RangedAttack() { }

        public RangedAttack(Actor owner)
            : base(owner)
        {
            IsIntrinsic = true;
        }

        public override bool IsEnabled<TOwner>(TOwner owner, RpgEntity initiator)
            => true;

        public ModSet OnCost(Actor owner, int focusPoints)
        {
            return new ModSet(new TimeLifecycle(TimePoints.BeginningOfEncounter))
                .AddMod(new TurnMod(), owner, x => x.CurrentFocusPoints, -focusPoints);
        }

        public ModSet OnAct(Actor owner, int focusPoints, int? abilityScore)
        {
            var modSet = new ModSet(new TimeLifecycle(TimePoints.Encounter(1)));

            ModCheck(modSet, "2d6");
            ModCheck(modSet, focusPoints);
            ModCheck(modSet, x => x.RangedAimBonus);

            if (abilityScore != null)
                ModCheck(modSet, abilityScore.Value);
            else
                ModCheck(modSet, x => x.RangedAttack);

            return modSet;
        }

        public ModSet[] OnOutcome(Actor owner, int diceRoll, int targetDefence)
        {
            var res = new List<ModSet>()
            {
                owner.GetState(nameof(Moving))!
            };

            return res.ToArray();
        }

    }
}
