using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Mods;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Models;
using Rpg.ModObjects.Values;

namespace Rpg.ModObjects.Tests
{
    public class DescriptionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void DescribeMelee()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var desc = entity.Describe("Melee");
            Assert.That(desc, Is.Not.Null);

            Assert.That(desc.EntityId, Is.EqualTo(entity.Id));
            Assert.That(desc.EntityArchetype, Is.EqualTo(entity.Archetype));
            Assert.That(desc.EntityName, Is.EqualTo(entity.Name));
            Assert.That(desc.Prop, Is.EqualTo("Melee"));
            Assert.That(desc.Value.Roll(), Is.EqualTo(4));
            Assert.That(desc.Mods.Count(), Is.EqualTo(2));

            var initialMod = desc.Mods.FirstOrDefault(x => x.ModType == nameof(Initial));

            Assert.That(initialMod, Is.Not.Null);
            Assert.That(initialMod.Value.Roll(), Is.EqualTo(2));

            var baseMods = desc.Mods.Where(x => x.ModType == nameof(Base));
            Assert.That(baseMods.Count(), Is.EqualTo(1));

            var baseMod = baseMods.First();
            Assert.That(baseMod.SourceProp!.Prop, Is.EqualTo("Bonus"));
            Assert.That(baseMod.SourceProp!.Value.Roll(), Is.EqualTo(2));
            Assert.That(baseMod.SourceProp!.Mods.Count(), Is.EqualTo(2));

            var scoreMod = baseMod.SourceProp!.Mods.FirstOrDefault(x => x.SourceProp?.Prop == "Score");
            Assert.That(scoreMod.SourceProp!.Prop, Is.EqualTo("Score"));
            Assert.That(scoreMod.Value.Roll(), Is.EqualTo(2));
            Assert.That(scoreMod.SourceProp.Value.Roll(), Is.EqualTo(14));

        }

        [Test]
        public void DescribeModSet()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);

            var modSet = new ModSet(entity.Id, "Testing")
                .Add(entity, x => x.Melee, 1)
                .Add(entity, x => x.Missile, -2)
                .Add(entity, x => x.Damage, "3d6")
                .Add(entity, x => x.Health, x => x.Melee);

            modSet.OnCreating(graph, entity);
            var desc = modSet.Describe();

            Assert.That(desc, Is.Not.Null);
            Assert.That(desc.Name, Is.EqualTo("Testing"));
            Assert.That(desc.Values.Count(), Is.EqualTo(4));

            Assert.That(desc.Get(new Props.PropRef(entity.Id, "Melee")), Is.EqualTo(new Dice("1")));
            Assert.That(desc.Get(new Props.PropRef(entity.Id, "Missile")), Is.EqualTo(new Dice("-2")));
            Assert.That(desc.Get(new Props.PropRef(entity.Id, "Damage")), Is.EqualTo(new Dice("3d6")));
            Assert.That(desc.Get(new Props.PropRef(entity.Id, "Health")), Is.EqualTo(new Dice("5")));
        }
    }
}
