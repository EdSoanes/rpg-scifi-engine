using Rpg.ModObjects.Meta.Props;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Server.Json;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Newtonsoft.Json;

namespace Rpg.ModObjects.Server.Tests
{
    public class DamageValue : RpgComponent
    {
        [JsonProperty]
        [Dice]
        public Dice Dice { get; protected set; }

        [JsonProperty]
        [Percent]
        public int ArmorPenetration { get; protected set; }

        [JsonProperty]
        [Meters]
        public int Radius { get; protected set; }

        [JsonConstructor] public DamageValue() : base() { }

        public DamageValue(string name, Dice dice, int armorPenetration, int radius)
            : base(name)
        {
            Dice = dice;
            ArmorPenetration = armorPenetration;
            Radius = radius;
        }
    }

    public class ScoreBonusValue : RpgComponent
    {
        [JsonProperty]
        [Integer(DataTypeName = "Score")]
        public int Score { get; protected set; }

        [JsonProperty] public int Bonus { get; protected set; }

        [JsonConstructor] public ScoreBonusValue() : base() { }

        public ScoreBonusValue(string name, int score)
            : base(name)
        {
            Score = score;
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            this.AddMod(new Base(), x => x.Bonus, x => x.Score, () => CalculateStatBonus);
        }

        public Dice CalculateStatBonus(Dice dice) => (int)Math.Floor((double)(dice.Roll() - 10) / 2);
    }

    public class ModdableEntity : RpgEntity
    {
        public ScoreBonusValue Strength { get; set; }
        public DamageValue Damage { get; set; }
        public Dice Melee { get; protected set; } = 2;
        public Dice Missile { get; protected set; }
        public int Health { get; protected set; } = 10;

        public RpgObjectCollection Objects { get; protected set; }

        public ModdableEntity()
        {
            Strength = new ScoreBonusValue(nameof(Strength), 14);
            Damage = new DamageValue(nameof(Damage), "d6", 10, 100);
            Objects = new RpgObjectCollection(Id, nameof(Objects));
        }

        public ModdableEntity(string name)
            : base(name)
        {
            Strength = new ScoreBonusValue(nameof(Strength), 14);
            Damage = new DamageValue(nameof(Damage), "d6", 10, 100);
            Objects = new RpgObjectCollection(Id, nameof(Objects));
        }

        public override void OnCreating(RpgGraph graph, RpgObject? entity = null)
        {
            base.OnCreating(graph, entity);
            Objects.OnCreating(graph, entity);
        }

        public override void OnRestoring(RpgGraph graph, RpgObject? entity)
        {
            base.OnRestoring(graph, entity);
            Objects.OnRestoring(graph, entity);
        }

        public override void OnTimeBegins()
        {
            base.OnTimeBegins();
            Objects.OnTimeBegins();
            this
                .AddMod(new Base(), x => x.Melee, x => x.Strength.Bonus)
                .AddMod(new Base(), x => x.Damage.Dice, x => x.Strength.Bonus);
        }

        public override LifecycleExpiry OnStartLifecycle()
        {
            base.OnStartLifecycle();
            Objects.OnStartLifecycle();
            return Expiry;
        }

        public override LifecycleExpiry OnUpdateLifecycle()
        {
            base.OnUpdateLifecycle();
            Objects.OnUpdateLifecycle();
            return Expiry;
        }
    }

    public class TestEntitySerializationTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void TestEntity_Serialize_EnsureValues()
        {
            var entity = new ModdableEntity();
            entity.Objects.Add(new ModdableEntity("Child"));

            var graph = new RpgGraph(entity);

            var json = RpgJson.Serialize(graph.GetGraphState());

            var graphState = RpgJson.Deserialize<RpgGraphState>(json)!;
            var graph2 = new RpgGraph(graphState);

            var entity2 = graph2.Context as ModdableEntity;

            Assert.That(entity2, Is.Not.Null);

            Assert.That(entity2.Strength.Score, Is.EqualTo(entity.Strength.Score));
            Assert.That(entity2.Strength.Bonus, Is.EqualTo(entity.Strength.Bonus));

            Assert.That(entity2.Damage.ArmorPenetration, Is.EqualTo(entity.Damage.ArmorPenetration));
            Assert.That(entity2.Damage.Dice, Is.EqualTo(entity.Damage.Dice));
            Assert.That(entity2.Damage.Radius, Is.EqualTo(entity.Damage.Radius));

            Assert.That(entity2.Melee, Is.EqualTo(entity.Melee));
            Assert.That(entity2.Missile, Is.EqualTo(entity.Missile));
        }

        [Test]
        public void RpgObjectCollection_Serialization()
        {
            var objs = new RpgObjectCollection();
            objs.Add(new ModdableEntity());
            var json = RpgJson.Serialize(objs);

            var objs2 = RpgJson.Deserialize<RpgObjectCollection>(json);
            Assert.That(objs2, Is.Not.Null);


        }
    }
}
