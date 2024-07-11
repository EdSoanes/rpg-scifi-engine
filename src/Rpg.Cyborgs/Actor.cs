using Newtonsoft.Json;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgEntity
    {
        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Strength { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Agility { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Health { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Brains { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Insight { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Stats")]
        public int Charisma { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1, Ignore = true)]
        public int FocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentFocusPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int LuckPoints { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentLuckPoints { get; protected set; }

        [JsonProperty]
        [Integer(Ignore = true)]
        public int Reactions { get; protected set; } = 7;

        [JsonProperty]
        [Threshold(Min = 1, Ignore = true)]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        [Integer()]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [Component(Ignore = true)]
        public BodyPart Head { get; protected set; }

        [JsonProperty]
        [Component(Ignore = true)]
        public BodyPart Torso { get; protected set; }

        [JsonProperty]
        public BodyPart LeftArm { get; protected set; }

        [JsonProperty]
        public BodyPart RightArm { get; protected set; }

        [JsonProperty]
        public BodyPart LeftLeg { get; protected set; }

        [JsonProperty]
        public BodyPart RightLeg { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1, Ignore = true)]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentLifePoints { get; protected set; }


        [JsonProperty]
        [Integer(Ignore = true)]
        public int Defence { get; protected set; } = 7;

        [JsonProperty]
        [Integer(Ignore = true)]
        public int ArmourRating { get; protected set; } = 6;

        [JsonProperty]
        [Integer(Ignore = true)]
        public int UnarmedDamageBonus { get; protected set; }
        
        [JsonProperty]
        [Integer(Ignore = true)]
        public int ParryDamageReduction { get; protected set; }

        [JsonProperty]
        [Integer(Ignore = true)]
        public int RangedAttack { get; protected set; }

        [JsonProperty]
        [Integer(Ignore = true)]
        public int RangedAimBonus { get; protected set; }

        [JsonProperty]
        [Integer(Ignore = true)]
        public int MeleeAttack { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int ActionPoints { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentActionPoints { get; protected set; }

        [JsonProperty]
        [Container(Tab = "Gear")]
        public RpgContainer Hands { get; protected set; }

        [JsonProperty]
        [Container(Tab = "Gear")]
        public RpgContainer Wearing { get; protected set; }

        [JsonConstructor] protected Actor()
        {
            Hands ??= new RpgContainer(Id, nameof(Hands));
            Wearing ??= new RpgContainer(Id, nameof(Wearing));

            Head ??= new BodyPart(Id, nameof(Head));
            Torso ??= new BodyPart(Id, nameof(Torso));
            LeftArm ??= new BodyPart(Id, nameof(LeftArm));
            RightArm ??= new BodyPart(Id, nameof(RightArm));
            LeftLeg ??= new BodyPart(Id, nameof(LeftLeg));
            RightLeg ??= new BodyPart(Id, nameof(RightLeg));
        }

        public Actor(string name)
            : this() 
        {
            Name = name;
        }

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

            this.BaseMod(x => x.CurrentActionPoints, x => x.ActionPoints);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
