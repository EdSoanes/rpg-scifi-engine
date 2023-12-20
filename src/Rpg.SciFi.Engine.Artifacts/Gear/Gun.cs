using Newtonsoft.Json;
using Rpg.SciFi.Engine.Artifacts.Components;
using Rpg.SciFi.Engine.Artifacts.Core;
using Rpg.SciFi.Engine.Artifacts.Expressions;
using Rpg.SciFi.Engine.Artifacts.Modifiers;
using Rpg.SciFi.Engine.Artifacts.Turns;

namespace Rpg.SciFi.Engine.Artifacts.Gear
{
    public class Gun : Artifact
    {
        private readonly int _baseRange;
        private readonly int _baseAttack;

        public Gun(int baseRange, int baseAttack)
        {
            _baseRange = baseRange;
            _baseAttack = baseAttack;
        }

        [JsonProperty] public Damage Damage { get; private set; } = new Damage();

        [Moddable] public int BaseRange { get => this.Resolve(nameof(BaseRange)); }
        [Moddable] public int BaseAttack { get => this.Resolve(nameof(BaseAttack)); }

        [Moddable] public int Range { get => this.Resolve(nameof(Range)); }
        [Moddable] public int Attack { get => this.Resolve(nameof(Attack)); }

        [Ability()]
        [Input(Param = "character", BindsTo = "Character")]
        [Input(InputSource = InputSource.Player, Param = "target")]
        [Input(InputSource = InputSource.Player, Param = "range")]
        public TurnAction Fire(Character character, Artifact target, int range)
        {
            var action = Context.CreateTurnAction(nameof(Fire), 3, 1, 1)
                .OnDiceRoll("d20")
                .OnDiceRoll(character, (x) => x.Stats.MissileAttackBonus)
                .OnDiceRoll(this, (x) => x.Attack)
                .OnDiceRoll(nameof(range), range, () => this.CalculateRange);

            action
                .OnDiceRollTarget(10)
                .OnDiceRollTarget(target, (x) => x.MissileDefence);

            action
                .OnSuccess(this.Mod((x) => x.Damage.Blast, target, (t) => t.Health.Physical).IsAdditive())
                .OnSuccess(this.Mod((x) => x.Damage.Impact, target, (t) => t.Health.Physical).IsAdditive())
                .OnSuccess(this.Mod((x) => x.Damage.Pierce, target, (t) => t.Health.Physical).IsAdditive());

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
                this.Mod(nameof(BaseRange), _baseRange, (x) => x.BaseRange),
                this.Mod(nameof(BaseAttack), _baseAttack, (x) => x.BaseAttack),

                this.Mod((x) => x.BaseRange, (x) => x.Range),
                this.Mod((x) => x.BaseAttack, (x) => x.Attack)
            };

            return mods.ToArray();
        }
    }
}
