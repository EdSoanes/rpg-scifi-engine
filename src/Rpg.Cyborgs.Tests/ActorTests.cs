using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Mods;
using Rpg.ModObjects.Mods.Templates;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;

namespace Rpg.Cyborgs.Tests
{
    internal class ActorTests
    {
        [SetUp]
        public void Setup()
        {
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);
        }

        [Test]
        public void Benny_EnsureBaseValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength.Value, Is.EqualTo(-1));
            Assert.That(pc.Agility.Value, Is.EqualTo(0));
            Assert.That(pc.Health.Value, Is.EqualTo(1));
            Assert.That(pc.Brains.Value, Is.EqualTo(1));
            Assert.That(pc.Insight.Value, Is.EqualTo(0));
            Assert.That(pc.Charisma.Value, Is.EqualTo(1));

            Assert.That(pc.StaminaPoints, Is.EqualTo(14));
            Assert.That(pc.LifePoints, Is.EqualTo(5));
            Assert.That(pc.FocusPoints, Is.EqualTo(1));
            Assert.That(pc.LuckPoints, Is.EqualTo(2));

            Assert.That(pc.Defence.Value, Is.EqualTo(7));
            Assert.That(pc.ArmourRating.Value, Is.EqualTo(6));
            Assert.That(pc.Reactions.Value, Is.EqualTo(7));
            Assert.That(pc.MeleeAttack.Value, Is.EqualTo(-1));
            Assert.That(pc.RangedAttack.Value, Is.EqualTo(0));

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);
        }

        [Test]
        public void Benny_EnsureActions()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.Actions.Count(), Is.EqualTo(6));
        }

        [Test]
        public void Benny_EnsureStates()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.States.Count(), Is.EqualTo(7));
            Assert.That(pc.States.Values.Where(x => x.IsOn).Count(), Is.EqualTo(0));
        }

        [Test]
        public void Benny_SetExhausted_EnsureStateOn()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            var exhausted = pc.GetState(nameof(Exhausted));
            Assert.That(exhausted, Is.Not.Null);

            exhausted.On();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnConditionally, Is.False);

            graph.Time.SetTime(TimePoints.Encounter(4));
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(exhausted.IsOnManually, Is.True);
            Assert.That(exhausted.IsOnConditionally, Is.False);

            exhausted.Off();
            graph.Time.TriggerEvent();

            Assert.That(exhausted.IsOn, Is.False);
            Assert.That(exhausted.IsOnManually, Is.False);
            Assert.That(exhausted.IsOnConditionally, Is.False);
        }

        [Test]
        public void Benny_Add4ToAgility_EnsureVeryFast()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var graph = new RpgGraph(pc);

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);

            pc.AddMod(new PermanentMod(), x => x.Agility.Value, 4);
            graph.Time.TriggerEvent();

            Assert.That(pc.ActionPoints, Is.EqualTo(2));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(2));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
        }

        [Test]
        public void Benny_CarriesSword_EnsureSerialization()
        {
            var sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            pc.Hands.Add(sword);

            var graph = new RpgGraph(pc);
            var json = RpgSerializer.Serialize(pc);

            Assert.That(json, Is.Not.Null);
        }
    }
}
