using Rpg.Cyborgs.Components;
using Rpg.ModObjects;
using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Values;
using System.Text.Json.Serialization;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgEntity
    {
        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Strength { get; protected set; } = new PropValue();

        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Agility { get; protected set; } = new PropValue();

        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Health { get; protected set; } = new PropValue();

        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Brains { get; protected set; } = new PropValue();

        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Insight { get; protected set; } = new PropValue();

        [JsonInclude]
        [Component(Group = "Stats")]
        public PropValue Charisma { get; protected set; } = new PropValue();


        [JsonInclude]
        [Threshold(Min = 1, Ignore = true)]
        public int FocusPoints { get; protected set; }

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentFocusPoints { get; protected set; }

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int LuckPoints { get; protected set; } = 1;

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentLuckPoints { get; protected set; }

        [JsonInclude]
        [Threshold(Min = 1, Ignore = true)]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        [Integer()]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonInclude]
        [Threshold(Min = 1, Ignore = true)]
        public int LifePoints { get; protected set; } = 6;

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentLifePoints { get; protected set; }

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int ActionPoints { get; protected set; } = 1;

        [JsonInclude]
        [Threshold(Min = 0, Ignore = true)]
        public int CurrentActionPoints { get; protected set; }


        [JsonInclude]
        public BodyPart Head { get; protected set; } = new BodyPart(nameof(Head), BodyPartType.Head);

        [JsonInclude]
        public BodyPart Torso { get; protected set; } = new BodyPart(nameof(Torso), BodyPartType.Torso);

        [JsonInclude]
        public BodyPart LeftArm { get; protected set; } = new BodyPart(nameof(LeftArm), BodyPartType.Limb);

        [JsonInclude]
        public BodyPart RightArm { get; protected set; } = new BodyPart(nameof(RightArm), BodyPartType.Limb);

        [JsonInclude]
        public BodyPart LeftLeg { get; protected set; } = new BodyPart(nameof(LeftLeg), BodyPartType.Limb);

        [JsonInclude]
        public BodyPart RightLeg { get; protected set; } = new BodyPart(nameof(RightLeg), BodyPartType.Limb);


        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue Reactions { get; protected set; } = new PropValue(7);

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue Defence { get; protected set; } = new PropValue(7);

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue ArmourRating { get; protected set; } = new PropValue(6);

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue UnarmedDamageBonus { get; protected set; } = new PropValue();

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue ParryDamageReduction { get; protected set; } = new PropValue();

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue RangedAttack { get; protected set; } = new PropValue();

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue RangedAimBonus { get; protected set; } = new PropValue();

        [JsonInclude]
        [Integer(Ignore = true)]
        public PropValue MeleeAttack { get; protected set; } = new PropValue();


        [JsonInclude]
        [Container(Tab = "Gear")]
        public RpgContainer Hands { get; protected set; } = new RpgContainer(nameof(Hands));

        [JsonInclude]
        [Container(Tab = "Gear")]
        public RpgContainer Wearing { get; protected set; } = new RpgContainer(nameof(Wearing));

        [JsonConstructor] protected Actor() { }

        public Actor(string name)
        {
            Name = name;
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();

            this.AddMod(new Base(), x => x.StaminaPoints, x => x.Health.Value, () => CalculateStamina);
            this.AddMod(new Base(), x => x.CurrentStaminaPoints, x => x.StaminaPoints);

            this.AddMod(new Base(), x => x.LifePoints, x => x.Strength.Value);
            this.AddMod(new Base(), x => x.CurrentLifePoints, x => x.LifePoints);

            this.AddMod(new Base(), x => x.FocusPoints, x => x.Agility.Value);
            this.AddMod(new Base(), x => x.FocusPoints, x => x.Brains.Value);
            this.AddMod(new Base(), x => x.FocusPoints, x => x.Insight.Value);
            this.AddMod(new Base(), x => x.CurrentFocusPoints, x => x.FocusPoints);

            this.AddMod(new Base(), x => x.LuckPoints, x => x.Charisma.Value);
            this.AddMod(new Base(), x => x.CurrentLuckPoints, x => x.LuckPoints);

            this.AddMod(new Base(), x => x.CurrentActionPoints, x => x.ActionPoints);

            this.AddMod(new Base(), x => x.Defence.Value, x => x.Agility.Value);
            this.AddMod(new Base(), x => x.Reactions.Value, x => x.Agility.Value);
            this.AddMod(new Base(), x => x.Reactions.Value, x => x.Insight.Value);

            this.AddMod(new Base(), x => x.ParryDamageReduction.Value, x => x.Strength.Value);
            this.AddMod(new Base(), x => x.RangedAttack.Value, x => x.Agility.Value);
            this.AddMod(new Base(), x => x.MeleeAttack.Value, x => x.Strength.Value);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
