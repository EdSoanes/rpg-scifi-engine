using Rpg.Cyborgs.Skills.Combat;
using Rpg.Cyborgs.Skills.Movement;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.System;

namespace Rpg.Cyborgs.Tests
{
    internal class ActorTests
    {
        private RpgSystem _system;

        [SetUp]
        public void Setup()
        {
            _system = TestSystem.Build();
        }

        [Test]
        public void Benny_EnsureBaseValues()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            Assert.That(pc.Name, Is.EqualTo("Benny"));
            Assert.That(pc.Strength, Is.EqualTo(-1));
            Assert.That(pc.Agility, Is.EqualTo(0));
            Assert.That(pc.Health, Is.EqualTo(1));
            Assert.That(pc.Brains, Is.EqualTo(1));
            Assert.That(pc.Insight, Is.EqualTo(0));
            Assert.That(pc.Charisma, Is.EqualTo(1));

            Assert.That(pc.StaminaPoints, Is.EqualTo(14));
            Assert.That(pc.CurrentStaminaPoints, Is.EqualTo(14));
            Assert.That(pc.LifePoints, Is.EqualTo(5));
            Assert.That(pc.CurrentLifePoints, Is.EqualTo(5));
            Assert.That(pc.FocusPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentFocusPoints, Is.EqualTo(1));
            Assert.That(pc.LuckPoints, Is.EqualTo(2));
            Assert.That(pc.CurrentLuckPoints, Is.EqualTo(2));

            Assert.That(pc.Defence, Is.EqualTo(7));
            Assert.That(pc.ArmourRating, Is.EqualTo(6));
            Assert.That(pc.Reactions, Is.EqualTo(7));
            Assert.That(pc.MeleeAttack, Is.EqualTo(-1));
            Assert.That(pc.RangedAttack, Is.EqualTo(0));
            Assert.That(pc.ParryDamageReduction, Is.EqualTo(-1));

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);
        }

        [Test]
        public void Benny_EnsureBodyParts()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            foreach (var bodyPart in new[] { pc.Head, pc.Torso, pc.LeftArm, pc.RightArm, pc.LeftLeg, pc.RightLeg })
            {
                Assert.That(characterSheet.ObjectExists(bodyPart.Id), Is.True);
                Assert.That(bodyPart.InjurySeverity, Is.EqualTo(0));
                Assert.That(bodyPart.Injuries.Length, Is.EqualTo(0));
                Assert.That(bodyPart.ActiveStates, Is.Empty);
            }
        }

        [Test]
        public void Benny_EnsureActions()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            var actions = characterSheet.GetObjectActions(pc.Id).Select(x => x.Name).Order().ToArray();
            Assert.That(actions, Is.EqualTo(new[] { "Aim", "ArmourCheck", "MeleeParry", "Run", "TakeDamage", "TakeInjury" }));
        }

        [Test]
        public void Benny_EnsureSkills()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            var aim = characterSheet.GetObjectAction(pc.Id, nameof(Aim)) as Aim;
            var run = characterSheet.GetObjectAction(pc.Id, nameof(Run)) as Run;

            Assert.That(aim, Is.Not.Null);
            Assert.That(aim.Classification, Is.EqualTo("Skill"));
            Assert.That(aim.Rating(characterSheet), Is.EqualTo(1));
            Assert.That(aim.IsPerformable, Is.True);

            Assert.That(run, Is.Not.Null);
            Assert.That(run.Rating(characterSheet), Is.EqualTo(0));
            Assert.That(run.IsPerformable, Is.True);

            //Ratings can be modded like any other property
            characterSheet.Add(new Rpg.Experimental.Mods.Standard(), pc, aim.RatingProp, 2);
            characterSheet.Time.Refresh();

            Assert.That(aim.Rating(characterSheet), Is.EqualTo(3));
        }

        [Test]
        public void Benny_EnsureStates()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            var states = characterSheet.GetObjectStates(pc.Id);

            Assert.That(states.Count(), Is.EqualTo(10));
            Assert.That(states.Count(x => x.IsOn), Is.EqualTo(0));
            Assert.That(states.Count(x => x.Classification == "Condition"), Is.EqualTo(2));
            Assert.That(pc.ActiveStates, Is.Empty);
        }

        [Test]
        public void Benny_SetExhausted_EnsureStateOn()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            var exhausted = characterSheet.GetObjectState(pc.Id, nameof(Exhausted));
            Assert.That(exhausted, Is.Not.Null);
            Assert.That(exhausted.IsOn, Is.False);

            exhausted.UserEnabled();
            characterSheet.Time.Refresh();

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(pc.IsStateOn(nameof(Exhausted)), Is.True);

            characterSheet.Time.ToTurn(4);

            Assert.That(exhausted.IsOn, Is.True);
            Assert.That(pc.IsStateOn(nameof(Exhausted)), Is.True);

            exhausted.UserEnabledReset();
            characterSheet.Time.Refresh();

            Assert.That(exhausted.IsOn, Is.False);
            Assert.That(pc.IsStateOn(nameof(Exhausted)), Is.False);
        }

        [Test]
        public void Benny_Add4ToAgility_EnsureVeryFast()
        {
            var pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            var characterSheet = new RpgCharacterSheet(pc, _system);

            Assert.That(pc.ActionPoints, Is.EqualTo(1));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(1));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.False);

            characterSheet.Add(pc, x => x.Agility, 4);
            characterSheet.Time.Refresh();

            Assert.That(pc.Agility, Is.EqualTo(4));
            Assert.That(pc.Reactions, Is.EqualTo(11));
            Assert.That(pc.Defence, Is.EqualTo(11));
            Assert.That(pc.FocusPoints, Is.EqualTo(5));
            Assert.That(pc.CurrentFocusPoints, Is.EqualTo(5));

            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
            Assert.That(pc.ActionPoints, Is.EqualTo(2));
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(2));
        }
    }
}
