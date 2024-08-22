using Newtonsoft.Json;
using Rpg.Cyborgs.Components;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgEntity
    {
        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Strength { get; protected set; } = new PropValue();

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Agility { get; protected set; } = new PropValue();

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Health { get; protected set; } = new PropValue();

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Brains { get; protected set; } = new PropValue();

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Insight { get; protected set; } = new PropValue();

        [JsonProperty]
        [Component(Group = "Stats")]
        public PropValue Charisma { get; protected set; } = new PropValue();


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
        [Threshold(Min = 1, Ignore = true)]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        [Integer()]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 1, Ignore = true)]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentLifePoints { get; protected set; }

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int ActionPoints { get; protected set; } = 1;

        [JsonProperty]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentActionPoints { get; protected set; }


        [JsonProperty]
        public BodyPart Head { get; protected set; } = new BodyPart();

        [JsonProperty]
        public BodyPart Torso { get; protected set; } = new BodyPart();

        [JsonProperty]
        public BodyPart LeftArm { get; protected set; } = new BodyPart();

        [JsonProperty]
        public BodyPart RightArm { get; protected set; } = new BodyPart();

        [JsonProperty]
        public BodyPart LeftLeg { get; protected set; } = new BodyPart();

        [JsonProperty]
        public BodyPart RightLeg { get; protected set; } = new BodyPart();


        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue Reactions { get; protected set; } = new PropValue(7);

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue Defence { get; protected set; } = new PropValue(7);

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue ArmourRating { get; protected set; } = new PropValue(6);

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue UnarmedDamageBonus { get; protected set; } = new PropValue();

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue ParryDamageReduction { get; protected set; } = new PropValue();

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue RangedAttack { get; protected set; } = new PropValue();

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue RangedAimBonus { get; protected set; } = new PropValue();

        [JsonProperty]
        [Integer(Ignore = true)]
        public PropValue MeleeAttack { get; protected set; } = new PropValue();


        [JsonProperty]
        [Container(Tab = "Gear")]
        public RpgContainer Hands { get; protected set; } = new RpgContainer();

        [JsonProperty]
        [Container(Tab = "Gear")]
        public RpgContainer Wearing { get; protected set; } = new RpgContainer();

        [JsonConstructor] protected Actor() { }

        public Actor(string name)
        {
            Name = name;
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();

            this.BaseMod(x => x.StaminaPoints, x => x.Health.Value, () => CalculateStamina);
            this.BaseMod(x => x.CurrentStaminaPoints, x => x.StaminaPoints);

            this.BaseMod(x => x.LifePoints, x => x.Strength.Value);
            this.BaseMod(x => x.CurrentLifePoints, x => x.LifePoints);

            this.BaseMod(x => x.FocusPoints, x => x.Agility.Value);
            this.BaseMod(x => x.FocusPoints, x => x.Brains.Value);
            this.BaseMod(x => x.FocusPoints, x => x.Insight.Value);
            this.BaseMod(x => x.CurrentFocusPoints, x => x.FocusPoints);

            this.BaseMod(x => x.LuckPoints, x => x.Charisma.Value);
            this.BaseMod(x => x.CurrentLuckPoints, x => x.LuckPoints);

            this.BaseMod(x => x.CurrentActionPoints, x => x.ActionPoints);

            this.BaseMod(x => x.Defence.Value, x => x.Agility.Value);
            this.BaseMod(x => x.Reactions.Value, x => x.Agility.Value);
            this.BaseMod(x => x.Reactions.Value, x => x.Insight.Value);

            this.BaseMod(x => x.ParryDamageReduction.Value, x => x.Strength.Value);
            this.BaseMod(x => x.RangedAttack.Value, x => x.Agility.Value);
            this.BaseMod(x => x.MeleeAttack.Value, x => x.Strength.Value);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
