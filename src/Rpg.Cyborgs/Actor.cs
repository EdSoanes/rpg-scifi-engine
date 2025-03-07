using Newtonsoft.Json;
using Rpg.Cyborgs.Components;
using Rpg.Experimental;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.System.Props;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgObject
    {
        [JsonProperty]
        public PropValue Strength { get; protected set; } = new PropValue(nameof(Strength));

        [JsonProperty]
        public PropValue Agility { get; protected set; } = new PropValue(nameof(Agility));

        [JsonProperty]
        public PropValue Health { get; protected set; } = new PropValue(nameof(Health));

        [JsonProperty]
        public PropValue Brains { get; protected set; } = new PropValue(nameof(Brains));

        [JsonProperty]
        public PropValue Insight { get; protected set; } = new PropValue(nameof(Insight));

        [JsonProperty]
        public PropValue Charisma { get; protected set; } = new PropValue(nameof(Charisma));

        [JsonProperty]
        [Integer(Min = 1)]
        public int FocusPoints { get; protected set; }

        [JsonProperty]
        [Integer(Min = 0)]
        public int CurrentFocusPoints { get; protected set; }

        [JsonProperty]
        [Integer(Min = 0)]
        public int LuckPoints { get; protected set; } = 1;

        [JsonProperty]
        [Integer(Min = 0)]
        public int CurrentLuckPoints { get; protected set; }

        [JsonProperty]
        [Integer(Min = 1)]
        public int StaminaPoints { get; protected set; } = 12;

        [JsonProperty]
        [Integer(Min = 0)]
        public int CurrentStaminaPoints { get; protected set; }

        [JsonProperty]
        [Integer(Min = 1)]
        public int LifePoints { get; protected set; } = 6;

        [JsonProperty]
        [Integer(Min = 0)]
        public int CurrentLifePoints { get; protected set; }

        [JsonProperty]
        [Integer(Min = 0)]
        public int ActionPoints { get; protected set; } = 1;

        [JsonProperty]
        [Integer(Min = 0)]
        public int CurrentActionPoints { get; protected set; }


        [JsonProperty]
        public BodyPart Head { get; protected set; } = new BodyPart(nameof(Head), BodyPartType.Head);

        [JsonProperty]
        public BodyPart Torso { get; protected set; } = new BodyPart(nameof(Torso), BodyPartType.Torso);

        [JsonProperty]
        public BodyPart LeftArm { get; protected set; } = new BodyPart(nameof(LeftArm), BodyPartType.Limb);

        [JsonProperty]
        public BodyPart RightArm { get; protected set; } = new BodyPart(nameof(RightArm), BodyPartType.Limb);

        [JsonProperty]
        public BodyPart LeftLeg { get; protected set; } = new BodyPart(nameof(LeftLeg), BodyPartType.Limb);

        [JsonProperty]
        public BodyPart RightLeg { get; protected set; } = new BodyPart(nameof(RightLeg), BodyPartType.Limb);


        [JsonProperty]
        public PropValue Reactions { get; protected set; } = new PropValue(nameof(Reactions), 7);

        [JsonProperty]
        public PropValue Defence { get; protected set; } = new PropValue(nameof(Defence), 7);

        [JsonProperty]
        public PropValue ArmourRating { get; protected set; } = new PropValue(nameof(ArmourRating), 6);

        [JsonProperty]
        public PropValue UnarmedDamageBonus { get; protected set; } = new PropValue(nameof(UnarmedDamageBonus));

        [JsonProperty]
        public PropValue ParryDamageReduction { get; protected set; } = new PropValue(nameof(ParryDamageReduction));

        [JsonProperty]
        public PropValue RangedAttack { get; protected set; } = new PropValue(nameof(RangedAttack));

        [JsonProperty]
        public PropValue RangedAimBonus { get; protected set; } = new PropValue(nameof(RangedAimBonus));

        [JsonProperty]
        public PropValue MeleeAttack { get; protected set; } = new PropValue(nameof(MeleeAttack));


        [JsonProperty]
        [Children(Tab = "Gear", MaxItems = 2)]
        public List<RpgObject> Hands { get; protected set; } = new();

        [JsonProperty]
        [Children(Tab = "Gear")]
        public List<RpgObject> Wearing { get; protected set; } = new();

        [JsonConstructor] protected Actor() { }

        public Actor(string name)
        {
            Name = name;
        }

        public override void OnCreating(RpgGraph graph, RpgObject? owner)
        {
            base.OnCreating(graph, owner);
            graph
                .Add(new Base(), this, x => x.StaminaPoints, x => x.Health.Value, () => CalculateStamina)
                .Add(new Base(), this, x => x.CurrentStaminaPoints, x => x.StaminaPoints)
                .Add(new Base(), this, x => x.LifePoints, x => x.Strength.Value)
                .Add(new Base(), this, x => x.CurrentLifePoints, x => x.LifePoints)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Agility.Value)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Brains.Value)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Insight.Value)
                .Add(new Base(), this, x => x.CurrentFocusPoints, x => x.FocusPoints)
                .Add(new Base(), this, x => x.LuckPoints, x => x.Charisma.Value)
                .Add(new Base(), this, x => x.CurrentLuckPoints, x => x.LuckPoints)
                .Add(new Base(), this, x => x.CurrentActionPoints, x => x.ActionPoints)
                .Add(new Base(), this, x => x.Defence.Value, x => x.Agility.Value)
                .Add(new Base(), this, x => x.Reactions.Value, x => x.Agility.Value)
                .Add(new Base(), this, x => x.Reactions.Value, x => x.Insight.Value)
                .Add(new Base(), this, x => x.ParryDamageReduction.Value, x => x.Strength.Value)
                .Add(new Base(), this, x => x.RangedAttack.Value, x => x.Agility.Value)
                .Add(new Base(), this, x => x.MeleeAttack.Value, x => x.Strength.Value);
        }

        public Dice CalculateStamina(Dice health)
            => health.Roll() * 2;
    }
}
