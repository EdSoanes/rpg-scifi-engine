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
        public Gun()
        {
            Name = nameof(Gun);
            Damage = new Damage("d6", "d8", 0, 0, 0);
        }

        [JsonProperty] public int BaseRange { get; private set; }
        [JsonProperty] public int BaseAttack { get; private set; }
        [JsonProperty] public Damage Damage { get; private set; }

        [Moddable] public int Range { get => this.Resolve(nameof(Range)); }
        [Moddable] public int Attack { get => this.Resolve(nameof(Attack)); }

        [Ability()]
        [Input(Param = "character", BindsTo = "Character")]
        [Input(InputSource = InputSource.Player, Param = "target")]
        [Input(InputSource = InputSource.Player, Param = "range")]
        public TurnAction Fire(Character character, Artifact target, int range)
        {
            var action = new TurnAction(3, 1, 1)
                .OnDiceRoll("d20")
                .OnDiceRoll(character, (x) => x.Stats.MissileAttackBonus)
                .OnDiceRoll(this, (x) => x.Attack)
                .OnDiceRoll(nameof(range), range, () => CalculateRange);

            action
                .OnDiceRollTarget(target, (x) => x.MissileToHit);

            action
                .OnSuccess(this.Mod((x) => x.Damage.Blast, target, (t) => t.Health.Physical).IsInstant())
                .OnSuccess(this.Mod((x) => x.Damage.Impact, target, (t) => t.Health.Physical).IsInstant())
                .OnSuccess(this.Mod((x) => x.Damage.Pierce, target, (t) => t.Health.Physical).IsInstant());

            return action;
        }

        public Dice CalculateRange(Dice range)
        {
            Dice res = -(int)Math.Floor((double)10 / Range * range.Roll()) + 2;
            return res;
        }

        [Setup]
        public override void Setup()
        {
            base.Setup();
            this.Mod((x) => x.Range, (x) => x.BaseRange).IsBase().Apply();
            this.Mod((x) => x.Attack, (x) => x.BaseAttack).IsBase().Apply();
        }
    }
}
