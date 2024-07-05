using Newtonsoft.Json;
using Rpg.Cyborgs.Attributes;
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
        [IntegerUI(Group = "Stats")]
        public int Strength { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Agility { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Health { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Brains { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Insight { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Charisma { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1)]
        [IntegerUI(Group = "Stats")]
        public int FocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int CurrentFocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Group = "Stats")]
        public int LuckPoints { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int CurrentLuckPoints { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Stats")]
        public int Reactions { get; protected set; } = 7;

        [JsonProperty]
        [Threshold(Min = 1)]
        [IntegerUI(Group = "Health")]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart Head { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart Torso { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart LeftArm { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart RightArm { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart LeftLeg { get; protected set; }

        [JsonProperty]
        [ComponentUI(Group = "Health")]
        public BodyPart RightLeg { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1)]
        [IntegerUI(Group = "Health")]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int CurrentLifePoints { get; protected set; }


        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int Defence { get; protected set; } = 7;

        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int ArmourRating { get; protected set; } = 6;

        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int UnarmedDamageBonus { get; protected set; }
        
        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int ParryDamageReduction { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int RangedAttack { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int RangedAimBonus { get; protected set; }

        [JsonProperty]
        [IntegerUI(Group = "Combat")]
        public int MeleeAttack { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int Actions { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0)]
        [IntegerUI(Ignore = true)]
        public int CurrentActions { get; protected set; }
        public RpgContainer Hands { get; protected set; } = new RpgContainer(nameof(Hands));
        public RpgContainer Wearing { get; protected set; } = new RpgContainer(nameof(Wearing));

        [JsonConstructor] protected Actor() { }

        public Actor(string name)
            : base(name) { }

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

            this.BaseMod(x => x.CurrentActions, x => x.Actions);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
