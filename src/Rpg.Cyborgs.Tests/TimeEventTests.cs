using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;
using Rpg.Experimental.Mods;
using Rpg.Experimental.System;
using Rpg.Experimental.Time;

namespace Rpg.Cyborgs.Tests
{
    /// <summary>
    /// The time events declared by the Cyborgs system: Sunrise and Sunset
    /// </summary>
    public class TimeEventTests
    {
        private RpgSystem _system;
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;

        [SetUp]
        public void Setup()
        {
            _system = TestSystem.Build();
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _characterSheet = new RpgCharacterSheet(_pc, _system);
        }

        [Test]
        public void Cyborgs_DeclaresSunriseAndSunset()
        {
            Assert.That(new CyborgsSystem().TimeEvents, Is.EqualTo(new[] { "Sunrise", "Sunset" }));
            Assert.That(_system.TimeEvents, Is.EqualTo(new[] { "Sunrise", "Sunset" }));
            Assert.That(_characterSheet.GetSystem().TimeEvents, Is.EqualTo(new[] { "Sunrise", "Sunset" }));
        }

        [Test]
        public void EffectUntilSunrise_EndsAtSunrise_NotAtSunset()
        {
            //Something that boosts strength until morning. Life points are derived from strength.
            _characterSheet.Add(new Standard().Until("Sunrise"), _pc, x => x.Strength, 2);
            _characterSheet.Time.Refresh();

            Assert.That(_characterSheet.Time.IsTurnTracking, Is.False);
            Assert.That(_pc.Strength, Is.EqualTo(1));
            Assert.That(_pc.LifePoints, Is.EqualTo(7));
            Assert.That(_pc.MeleeAttack, Is.EqualTo(1));

            _characterSheet.TriggerTimeEvent("Sunset");

            Assert.That(_characterSheet.Time.LastEvent, Is.EqualTo("Sunset"));
            Assert.That(_pc.Strength, Is.EqualTo(1));
            Assert.That(_pc.LifePoints, Is.EqualTo(7));

            _characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(_characterSheet.Time.LastEvent, Is.EqualTo("Sunrise"));
            Assert.That(_pc.Strength, Is.EqualTo(-1));
            Assert.That(_pc.LifePoints, Is.EqualTo(5));
            Assert.That(_pc.MeleeAttack, Is.EqualTo(-1));
        }

        [Test]
        public void EffectFromSunsetUntilSunrise_OnlyActiveAtNight()
        {
            var nightVision = new Standard()
                .Lifespan(TimePoint.AtEvent("Sunset"), TimePoint.AtEvent("Sunrise"));

            _characterSheet.Add(nightVision, _pc, x => x.Insight, 3);
            _characterSheet.Time.Refresh();

            Assert.That(_pc.Insight, Is.EqualTo(0));
            Assert.That(_pc.Reactions, Is.EqualTo(7));

            _characterSheet.TriggerTimeEvent("Sunset");

            Assert.That(_pc.Insight, Is.EqualTo(3));
            Assert.That(_pc.Reactions, Is.EqualTo(10));
            Assert.That(_pc.FocusPoints, Is.EqualTo(4));

            _characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(_pc.Insight, Is.EqualTo(0));
            Assert.That(_pc.Reactions, Is.EqualTo(7));
            Assert.That(_pc.FocusPoints, Is.EqualTo(1));
        }

        [Test]
        public void Sunrise_DuringAFight_EndsTheEffect_NotTheFight()
        {
            //Enough agility until sunrise to make Benny very fast
            _characterSheet.Add(new Standard().Until("Sunrise"), _pc, x => x.Agility, 4);
            _characterSheet.BeginTurnTracking();
            _characterSheet.AdvanceToTurn(3);

            Assert.That(_pc.IsStateOn(nameof(VeryFast)), Is.True);
            Assert.That(_pc.ActionPoints, Is.EqualTo(2));
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(2));

            _characterSheet.TriggerTimeEvent("Sunrise");

            Assert.That(_characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(_characterSheet.Time.Turn, Is.EqualTo(3));
            Assert.That(_pc.Agility, Is.EqualTo(0));
            Assert.That(_pc.IsStateOn(nameof(VeryFast)), Is.False);
            Assert.That(_pc.ActionPoints, Is.EqualTo(1));
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            //The GM replays the turn: the sun has not risen yet
            Assert.That(_characterSheet.RewindToTurn(3), Is.True);
            var pc = (_characterSheet.Actor as PlayerCharacter)!;

            Assert.That(pc.Agility, Is.EqualTo(4));
            Assert.That(pc.IsStateOn(nameof(VeryFast)), Is.True);
            Assert.That(pc.CurrentActionPoints, Is.EqualTo(2));
        }

        [Test]
        public void EffectUntilSunrise_SurvivesSaveAndRestore()
        {
            _characterSheet.Add(new Standard().Until("Sunrise"), _pc, x => x.Strength, 2);
            _characterSheet.TriggerTimeEvent("Sunset");

            var characterSheet2 = TestSystem.RoundTrip(_characterSheet);
            var pc2 = (characterSheet2.Actor as PlayerCharacter)!;

            Assert.That(characterSheet2.Time.LastEvent, Is.EqualTo("Sunset"));
            Assert.That(pc2.Strength, Is.EqualTo(1));

            characterSheet2.TriggerTimeEvent("Sunrise");

            Assert.That(pc2.Strength, Is.EqualTo(-1));
            Assert.That(pc2.LifePoints, Is.EqualTo(5));
        }
    }
}
