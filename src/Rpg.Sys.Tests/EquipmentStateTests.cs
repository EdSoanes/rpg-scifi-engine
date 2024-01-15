using Rpg.Sys.Archetypes;
using Rpg.Sys.Components;
using Rpg.Sys.Modifiers;

namespace Rpg.Sys.Tests
{
    public class EquipmentStateTests
    {
        private TestEquipment _equipment;
        private Graph _graph;
        private Actor _actor;

        [SetUp]
        public void Setup()
        {
            _graph = new Graph();
            _equipment = new TestEquipment(new ArtifactTemplate
            {
                Name = "Thing",
            });

            _actor = new Actor(new ActorTemplate
            {
                Name = "Ben",
                Health = new HealthTemplate
                {
                    Physical = 10
                }
            });

            _graph.Initialize(_actor);
            _graph.Entities.Add(_equipment);
        }

        [Test]
        public void Equipment_ActivateOnState_VerifySoundProperty()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_equipment.Presence.Sound.Current, Is.EqualTo(0));

            var action = _actor.ActivateState(_equipment, "On");
            action!.Resolve(_actor, _graph);

            Assert.That(_equipment.Presence.Sound.Current, Is.EqualTo(1));
        }

        [Test]
        public void Equipment_DeactivateOnState_VerifySoundProperty()
        {
            var action = _actor.ActivateState(_equipment, "On");
            action!.Resolve(_actor, _graph);

            Assert.That(_equipment.Presence.Sound.Current, Is.EqualTo(1));

            action = _actor.DeactivateState(_equipment, "On");
            action!.Resolve(_actor, _graph);

            Assert.That(_equipment.Presence.Sound.Current, Is.EqualTo(0));
        }

        [Test]
        public void Equipment_ActivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_actor, Is.Not.Null);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(10));

            var action = _actor.ActivateState(_equipment, "Enhance");
            action!.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(13));
        }

        [Test]
        public void Equipment_DeactivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_actor, Is.Not.Null);

            var action = _actor.ActivateState(_equipment, "Enhance");
            action!.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(13));

            action = _actor.DeactivateState(_equipment, "Enhance");
            action!.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(10));

            var healthMods = _graph.Mods.GetMods(_actor, x => x.Health.Physical.Current);

            Assert.NotNull(healthMods);
            var enhanceMods = healthMods.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);

            Assert.That(enhanceMods.Count(), Is.EqualTo(1));
        }

        [Test]
        public void Equipment_OnTurn2_DeactivateEnhanceActorState_VerifyActorPhysicalHealth()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_actor, Is.Not.Null);

            _graph.NewEncounter();

            var action = _actor.ActivateState(_equipment, "Enhance");
            action!.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(13));

            _graph.NewTurn();

            action = _actor.DeactivateState(_equipment, "Enhance");
            action!.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(10));

            var healthMods = _graph.Mods.GetMods(_actor, x => x.Health.Physical.Current);

            Assert.NotNull(healthMods);
            var enhanceMods = healthMods.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);

            Assert.That(enhanceMods.Count(), Is.EqualTo(1));
            Assert.That(enhanceMods.First().Duration.GetExpiry(_graph.Turn), Is.EqualTo(ModifierExpiry.Expired));
        }

        [Test]
        public void Equipment_ActivateEnhanceActorState_RemoveEquipmentRemovesMods()
        {
            Assert.That(_equipment, Is.Not.Null);
            Assert.That(_actor, Is.Not.Null);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(10));

            var action = _actor.ActivateState(_equipment, "Enhance");

            Assert.That(action, Is.Not.Null);
            action.Resolve(_actor, _graph);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(13));

            _graph.Entities.Remove(_equipment.Id);

            Assert.That(_actor.Health.Physical.Current, Is.EqualTo(10));

            var healthMods = _graph.Mods.GetMods(_actor, x => x.Health.Physical.Current);

            Assert.NotNull(healthMods);
            var enhanceMods = healthMods.Where(x => x.Name == "Enhance" && x.ModifierType == ModifierType.State);

            Assert.That(enhanceMods.Count(), Is.EqualTo(0));
        }
    }
}
