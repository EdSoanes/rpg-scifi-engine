using Rpg.Sys.Archetypes;
using Rpg.Sys.Modifiers;
using Rpg.Sys.Tests.Factories;

namespace Rpg.Sys.Tests
{
    public class EquipmentStateTests
    {
        [Test]
        public void Equipment_ActivateOnState_VerifySoundProperty()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(equipment, Is.Not.Null);
            Assert.That(equipment.Presence.Sound.Current, Is.EqualTo(0));

            var action = human.ActivateState(equipment, "On");
            action!.Resolve(human, graph);

            Assert.That(equipment.Presence.Sound.Current, Is.EqualTo(1));
        }

        [Test]
        public void Equipment_DeactivateOnState_VerifySoundProperty()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            var action = human.ActivateState(equipment, "On");
            action!.Resolve(human, graph);

            Assert.That(equipment.Presence.Sound.Current, Is.EqualTo(1));

            action = human.DeactivateState(equipment, "On");
            action!.Resolve(human, graph);

            Assert.That(equipment.Presence.Sound.Current, Is.EqualTo(0));
        }

        [Test]
        public void Equipment_ActivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(equipment, Is.Not.Null);
            Assert.That(human, Is.Not.Null);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(10));

            var action = human.ActivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(13));
        }

        [Test]
        public void Equipment_DeactivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(equipment, Is.Not.Null);
            Assert.That(human, Is.Not.Null);

            var action = human.ActivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(13));

            action = human.DeactivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(10));
        }

        [Test]
        public void Equipment_DeactivateEnhanceActorState_EnhanceModRemoved()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            var enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(0));

            var action = human.ActivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(1));

            action = human.DeactivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(0));
        }

        [Test]
        public void Equipment_OnTurn2_DeactivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            graph.NewEncounter();

            var action = human.ActivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(13));

            graph.NewTurn();

            action = human.DeactivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(10));
        }

        [Test]
        public void Equipment_OnTurn2_DeactivateEnhanceActorState_EnhanceModExpired()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            var enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(0));

            graph.NewEncounter();

            var action = human.ActivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(1));

            graph.NewTurn();

            action = human.DeactivateState(equipment, "Enhance");
            action!.Resolve(human, graph);

            enhanceMods = graph.Get.Mods(human, x => x.Health.Physical.Current)!.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);
            Assert.NotNull(enhanceMods);
            Assert.That(enhanceMods.Count(), Is.EqualTo(1));
            Assert.That(enhanceMods.First().Duration.GetExpiry(graph.Turn), Is.EqualTo(ModifierExpiry.Expired));
        }

        [Test]
        public void Equipment_ActivateEnhanceActorState_RemoveEquipmentRemovesMods()
        {
            var graph = HumanFactory.Create();
            var human = graph.GetContext<Human>();
            var equipment = human.RightHand.Get<TestEquipment>().Single();

            Assert.That(human.Health.Physical.Current, Is.EqualTo(10));

            var action = human.ActivateState(equipment, "Enhance");

            Assert.That(action, Is.Not.Null);
            action.Resolve(human, graph);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(13));

            graph.Remove.Entities(equipment);

            Assert.That(human.Health.Physical.Current, Is.EqualTo(10));

            var healthMods = graph.Get.Mods(human, x => x.Health.Physical.Current);

            Assert.NotNull(healthMods);
            var enhanceMods = healthMods.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);

            Assert.That(enhanceMods.Count(), Is.EqualTo(0));
        }
    }
}
