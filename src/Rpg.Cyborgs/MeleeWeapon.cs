using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public class MeleeWeapon : RpgEntity
    {
        [JsonProperty]
        [DiceUI]
        public Dice Damage { get; protected set; }

        [JsonProperty]
        [DiceUI]
        public int HitBonus { get; protected set; }

        [JsonConstructor] private MeleeWeapon() { }

        public MeleeWeapon(MeleeWeaponTemplate template)
            : base(template.Name)
        {
            Damage = template.Damage;
            HitBonus = template.HitBonus;
        }

        protected override void OnLifecycleStarting()
        {
            base.OnLifecycleStarting();
            this.InitActionsAndStates(Graph!);
        }
    }
}
