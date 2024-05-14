using Rpg.ModObjects.Modifiers;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class DescriptionTests
    {
        [SetUp]
        public void Setup()
        {
            ModGraphExtensions.RegisterAssembly(Assembly.GetExecutingAssembly());
        }

        [Test]
        public void DescribeMelee()
        {
            var entity = new ModdableEntity();
            var graph = new ModGraph(entity);

            var desc = entity.Describe(x => x.Melee);
            Assert.IsNotNull(desc);

            Assert.That(desc.Entity, Is.SameAs(entity));
            Assert.That(desc.Prop, Is.EqualTo("Melee"));
            Assert.That(desc.Value.Roll(), Is.EqualTo(4));
            Assert.That(desc.Mods.Count(), Is.EqualTo(2));

            var initialMod = desc.Mods.FirstOrDefault(x => x.ModType == ModType.BaseInit);

            Assert.That(initialMod, Is.Not.Null);
            Assert.That(initialMod.Value.Roll(), Is.EqualTo(2));

            var baseMods = desc.Mods.Where(x => x.ModType == ModType.Base);
            Assert.That(baseMods.Count(), Is.EqualTo(1));

            var baseMod = baseMods.First();
            Assert.That(baseMod.SourceProp!.Prop, Is.EqualTo("Bonus"));
            Assert.That(baseMod.SourceProp!.Value.Roll(), Is.EqualTo(2));
            Assert.That(baseMod.Mods.Count(), Is.EqualTo(1));

            var scoreMod = baseMod.Mods.First();
            Assert.That(scoreMod.SourceProp!.Prop, Is.EqualTo("Score"));
            Assert.That(scoreMod.Value.Roll(), Is.EqualTo(2));
            Assert.That(scoreMod.SourceValue.Roll(), Is.EqualTo(14));

        }
    }
}
