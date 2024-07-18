using Newtonsoft.Json;
using Rpg.Cyborgs.Components;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgEntity
    {
        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Strength { get; protected set; }

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Agility { get; protected set; }

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Health { get; protected set; }

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Brains { get; protected set; }

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Insight { get; protected set; }

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Charisma { get; protected set; }

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
            Strength = new PropValue(Id, nameof(Strength));
            Agility = new PropValue(Id, nameof(Agility));
            Health = new PropValue(Id, nameof(Health));
            Brains = new PropValue(Id, nameof(Brains));
            Insight = new PropValue(Id, nameof(Insight));
            Charisma = new PropValue(Id, nameof(Charisma));

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
            this.BaseMod(x => x.StaminaPoints, x => x.Health.Value, () => CalculateStamina);
            this.BaseMod(x => x.CurrentStaminaPoints, x => x.StaminaPoints);

            this.BaseMod(x => x.LifePoints, x => x.Strength.Value);
            this.BaseMod(x => x.CurrentLifePoints, x => x.LifePoints);

            this.BaseMod(x => x.FocusPoints, x => x.Agility.Value);
            this.BaseMod(x => x.FocusPoints, x => x.Brains.Value);
            this.BaseMod(x => x.FocusPoints, x => x.Insight.Value);
            this.BaseMod(x => x.CurrentFocusPoints, x => x.FocusPoints);

            this.BaseMod(x => x.Defence, x => x.Agility.Value);
            this.BaseMod(x => x.Reactions, x => x.Agility.Value);
            this.BaseMod(x => x.Reactions, x => x.Insight.Value);

            this.BaseMod(x => x.LuckPoints, x => x.Charisma.Value);
            this.BaseMod(x => x.CurrentLuckPoints, x => x.LuckPoints);

            this.BaseMod(x => x.ParryDamageReduction, x => x.Strength);
            this.BaseMod(x => x.RangedAttack, x => x.Agility.Value);
            this.BaseMod(x => x.MeleeAttack, x => x.Strength.Value);

            this.BaseMod(x => x.CurrentActionPoints, x => x.ActionPoints);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
