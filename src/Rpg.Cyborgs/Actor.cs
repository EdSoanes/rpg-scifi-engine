using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Attributes;

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
        [IntegerUI]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [IntegerUI]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [IntegerUI]
        public int CurrentLifePoints { get; protected set; } = 6;

        [JsonProperty]
        [IntegerUI]
        public int FocusPoints { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int CurrentFocusPoints { get; protected set; }

        [JsonProperty]
        [IntegerUI]
        public int LuckPoints { get; protected set; } = 1;

        [JsonProperty]
        [IntegerUI]
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

    }
}
