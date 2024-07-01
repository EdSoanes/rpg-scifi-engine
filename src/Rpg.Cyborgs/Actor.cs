using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Props.Attributes;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgEntity
    {
        [JsonProperty]
        [IntegerUI]
        public int Strength { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Agility { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Health { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Brains { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Insight { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Charisma { get; protected set; }


        [JsonProperty]
        [Threshold(Min = 1)]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [Threshold(Min = 0)]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1)]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [Threshold(Min = 0)]
        public int CurrentLifePoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1)]
        public int FocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        public int CurrentFocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        public int LuckPoints { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0)]
        public int CurrentLuckPoints { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int Defence { get; protected set; } = 7;

        [JsonProperty]
        [IntegerUI]
        public int Reactions { get; protected set; } = 7;

        [JsonProperty]
        [IntegerUI]
        public int ArmourRating { get; protected set; } = 6;

        [JsonProperty]
        [IntegerUI]
        public int UnarmedDamageBonus { get; protected set; }
        
        [JsonProperty]
        [IntegerUI]
        public int ParryDamageReduction { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int RangedAttack { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int RangedAimBonus { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int MeleeAttack { get; protected set; }

        protected override void OnLifecycleStarting()
        {
            this.BaseMod(x => x.StaminaPoints, x => x.Health, () => CalculateStamina);
            this.BaseMod(x => x.CurrentStaminaPoints, x => x.StaminaPoints);

            this.BaseMod(x => x.LifePoints, x => x.Strength);
            this.BaseMod(x => x.CurrentLifePoints, x => x.LifePoints);

            this.BaseMod(x => x.FocusPoints, x => x.Agility);
            this.BaseMod(x => x.FocusPoints, x => x.Brains);
            this.BaseMod(x => x.FocusPoints, x => x.Insight);
            this.BaseMod(x => x.CurrentFocusPoints, x => x.FocusPoints);

            this.BaseMod(x => x.Defence, x => x.Agility);
            this.BaseMod(x => x.Reactions, x => x.Agility);
            this.BaseMod(x => x.Reactions, x => x.Insight);

            this.BaseMod(x => x.LuckPoints, x => x.Charisma);
            this.BaseMod(x => x.CurrentLuckPoints, x => x.LuckPoints);

            this.BaseMod(x => x.ParryDamageReduction, x => x.Strength);
            this.BaseMod(x => x.RangedAttack, x => x.Agility);
            this.BaseMod(x => x.MeleeAttack, x => x.Strength);
        }

        public Dice CalculateStamina(Dice health)
            => 12 + (health.Roll() * 2);
    }
}
