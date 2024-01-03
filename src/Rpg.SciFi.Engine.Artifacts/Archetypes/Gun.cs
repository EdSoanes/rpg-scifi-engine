using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Actions;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;

namespace Rpg.SciFi.Engine.Artifacts.Archetypes
{
    public class Gun : Artifact
    {
        private readonly GunTemplate _template;

        public Gun(GunTemplate template)
        {
            Name = template.GetType().Name;
            _template = template;
        }

        [JsonProperty] public Damage Damage { get; private set; } = new Damage();

        [Moddable] public int BaseRange { get => Resolve(); }
        [Moddable] public int BaseAttack { get => Resolve(); }

        [Moddable] public int Range { get => Resolve(); }
        [Moddable] public int Attack { get => Resolve(); }

        [Ability()]
        [Input(Param = "character", BindsTo = "Character")]
        [Input(InputSource = InputSource.Player, Param = "target")]
        [Input(InputSource = InputSource.Player, Param = "range")]
        public TurnAction Fire(Character character, Artifact target, int range)
        {
            var action = new TurnAction(Graph!, nameof(Fire), 3, 1, 1)
                .OnDiceRoll("d20")
                .OnDiceRoll(character, (x) => x.Stats.MissileAttackBonus)
                .OnDiceRoll(this, (x) => x.Attack)
                .OnDiceRoll(nameof(range), range, () => CalculateRange);

            action
                .OnDiceRollTarget(10)
                .OnDiceRollTarget(target, (x) => x.MissileDefence);

            action
                //                .OnSuccess(DamageModifier.Create(this, x => x.Damage.Blast, target, t => t.Health.Physical))
                .OnSuccess(DamageModifier.Create(this, x => x.Damage.Impact, target, t => t.Health.Physical));
            //                .OnSuccess(DamageModifier.Create(this, x => x.Damage.Pierce, target, t => t.Health.Physical));

            return action;
        }

        public Dice CalculateRange(Dice range)
        {
            Dice res = -(int)Math.Floor((double)10 / Range * range.Roll()) + 2;
            return res;
        }

        [Setup]
        public override Modifier[] Setup()
        {
            var mods = new List<Modifier>(base.Setup())
            {
                BaseModifier.Create(this, _template.Impact, x => x.Damage.BaseImpact),
                BaseModifier.Create(this, _template.Range, x => x.BaseRange),
                BaseModifier.Create(this, _template.Attack, x => x.BaseAttack),

                BaseModifier.Create(this, x => x.BaseRange, x => x.Range),
                BaseModifier.Create(this, x => x.BaseAttack, x => x.Attack)
            };

            return mods.ToArray();
        }
    }
}
