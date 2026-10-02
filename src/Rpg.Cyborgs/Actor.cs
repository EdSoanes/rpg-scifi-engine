using Newtonsoft.Json;
using Rpg.Experimental;
using Rpg.Experimental.Description;
using Rpg.Experimental.Graph;
using Rpg.Experimental.Mods;
using Rpg.Experimental.System.Props;

namespace Rpg.Cyborgs
{
    public abstract class Actor : RpgObject
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
        [Integer(Group = "Combat")]
        public int Reactions { get; protected set; } = 7;

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int Defence { get; protected set; } = 7;

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int ArmourRating { get; protected set; } = 6;

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int UnarmedDamageBonus { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int ParryDamageReduction { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int RangedAttack { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int RangedAimBonus { get; protected set; }

        [JsonProperty]
        [Integer(Group = "Combat")]
        public int MeleeAttack { get; protected set; }


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
                .Add(new Base(), this, x => x.StaminaPoints, x => x.Health, () => CalculateStamina)
                .Add(new Base(), this, x => x.CurrentStaminaPoints, x => x.StaminaPoints)
                .Add(new Base(), this, x => x.LifePoints, x => x.Strength)
                .Add(new Base(), this, x => x.CurrentLifePoints, x => x.LifePoints)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Agility)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Brains)
                .Add(new Base(), this, x => x.FocusPoints, x => x.Insight)
                .Add(new Base(), this, x => x.CurrentFocusPoints, x => x.FocusPoints)
                .Add(new Base(), this, x => x.LuckPoints, x => x.Charisma)
                .Add(new Base(), this, x => x.CurrentLuckPoints, x => x.LuckPoints)
                .Add(new Base(), this, x => x.CurrentActionPoints, x => x.ActionPoints)
                .Add(new Base(), this, x => x.Defence, x => x.Agility)
                .Add(new Base(), this, x => x.Reactions, x => x.Agility)
                .Add(new Base(), this, x => x.Reactions, x => x.Insight)
                .Add(new Base(), this, x => x.ParryDamageReduction, x => x.Strength)
                .Add(new Base(), this, x => x.RangedAttack, x => x.Agility)
                .Add(new Base(), this, x => x.MeleeAttack, x => x.Strength);
        }

        [Describe("Twice health")]
        public Dice CalculateStamina(Dice health)
            => health * 2;
    }
}
