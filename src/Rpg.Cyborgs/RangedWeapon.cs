using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public class RangedWeapon : RpgEntity
    {
        [JsonProperty]
        [Dice]
        public Dice Damage { get; protected set; }

        [JsonProperty]
        [Integer]
        public int HitBonus { get; protected set; }

        [JsonConstructor] private RangedWeapon() { }

        public RangedWeapon(RangedWeaponTemplate template)
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
