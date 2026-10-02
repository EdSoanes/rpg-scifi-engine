using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.Experimental;

namespace Rpg.Cyborgs.Tests
{
    public class RpgGraphTests
    {
        private RpgCharacterSheet _characterSheet;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;

        [SetUp]
        public void Setup()
        {
            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);

            _characterSheet = new RpgCharacterSheet(room, _pc, TestSystem.Build());
        }

        [Test]
        public void BennyHasSword_EnsureSerialization()
        {
            var characterSheet2 = TestSystem.RoundTrip(_characterSheet);

            var room2 = characterSheet2.Context as Room;
            var pc2 = characterSheet2.Actor as PlayerCharacter;

            Assert.That(room2, Is.Not.Null);
            Assert.That(pc2, Is.Not.Null);
            Assert.That(pc2.Id, Is.EqualTo(_pc.Id));
            Assert.That(room2.Contents.Contains(pc2), Is.True);

            Assert.That(characterSheet2.Time.Now, Is.EqualTo(_characterSheet.Time.Now));
            Assert.That(characterSheet2.GetObjectCount(), Is.EqualTo(_characterSheet.GetObjectCount()));
            Assert.That(characterSheet2.GetObjectDataCount(), Is.EqualTo(_characterSheet.GetObjectDataCount()));

            Assert.That(pc2.Name, Is.EqualTo("Benny"));
            Assert.That(pc2.Strength, Is.EqualTo(-1));
            Assert.That(pc2.StaminaPoints, Is.EqualTo(14));
            Assert.That(pc2.CurrentStaminaPoints, Is.EqualTo(14));
            Assert.That(pc2.LifePoints, Is.EqualTo(5));
            Assert.That(pc2.Defence, Is.EqualTo(7));
            Assert.That(pc2.MeleeAttack, Is.EqualTo(-1));

            Assert.That(pc2.Head, Is.Not.Null);
            Assert.That(pc2.Head.Id, Is.EqualTo(_pc.Head.Id));
            Assert.That(pc2.Head.BodyPartType, Is.EqualTo(BodyPartType.Head));
            Assert.That(pc2.LeftArm.Id, Is.EqualTo(_pc.LeftArm.Id));

            Assert.That(pc2.Hands.Count, Is.EqualTo(1));
            Assert.That(pc2.Hands[0].Id, Is.EqualTo(_sword.Id));
            Assert.That((pc2.Hands[0] as MeleeWeapon)?.Damage.ToString(), Is.EqualTo("1d6"));

            Assert.That(characterSheet2.GetObjectStates(pc2.Id).Length, Is.EqualTo(10));
            Assert.That(characterSheet2.GetObjectActions(pc2.Id).Length, Is.EqualTo(6));
            Assert.That(characterSheet2.GetObjectAction(_sword.Id, nameof(MeleeAttack))?.IsPerformable, Is.True);
        }

        [Test]
        public void Restored_DerivedValuesAndStates_StillWork()
        {
            var characterSheet2 = TestSystem.RoundTrip(_characterSheet);
            var pc2 = (characterSheet2.Actor as PlayerCharacter)!;

            characterSheet2.Add(pc2, x => x.Agility, 4);
            characterSheet2.Time.Refresh();

            Assert.That(pc2.Reactions, Is.EqualTo(11));
            Assert.That(pc2.IsStateOn(nameof(VeryFast)), Is.True);
            Assert.That(pc2.ActionPoints, Is.EqualTo(2));
            Assert.That(pc2.CurrentActionPoints, Is.EqualTo(2));

            var characterSheet3 = TestSystem.RoundTrip(characterSheet2);
            var pc3 = (characterSheet3.Actor as PlayerCharacter)!;

            Assert.That(pc3.Agility, Is.EqualTo(4));
            Assert.That(pc3.IsStateOn(nameof(VeryFast)), Is.True);
            Assert.That(pc3.ActionPoints, Is.EqualTo(2));
            Assert.That(pc3.CurrentActionPoints, Is.EqualTo(2));
        }

        [Test]
        public void TakeDamageAndInjury_RoundTripBetweenEveryStep()
        {
            _characterSheet.Time.BeginEncounter();

            var sheet = _characterSheet;
            var activityId = sheet.CreateActivity(_pc.Id, _pc.Id, nameof(TakeDamage)).Id;

            RpgActivityAction Current()
                => (sheet.GetObject(activityId) as RpgActivity)!.CurrentActivityAction!;

            sheet = TestSystem.RoundTrip(sheet);
            Assert.That(Current().Outcome(sheet, ("damage", 15)), Is.True);

            sheet = TestSystem.RoundTrip(sheet);
            var nextActions = Current().Complete();
            sheet.Time.Refresh();

            Assert.That(nextActions.Length, Is.EqualTo(1));
            Assert.That(nextActions[0].ActionName, Is.EqualTo(nameof(TakeInjury)));

            sheet = TestSystem.RoundTrip(sheet);
            sheet.CreateActivity(_pc.Id, nextActions[0]);

            sheet = TestSystem.RoundTrip(sheet);
            Assert.That(Current().GetAction()?.Name, Is.EqualTo(nameof(TakeInjury)));
            Assert.That(Current().Perform(sheet), Is.True);

            sheet = TestSystem.RoundTrip(sheet);
            Assert.That(sheet.GetPropertyValue<Dice>(Current().Id, "injuryRoll").ToString(), Is.EqualTo("2d6 - 1"));
            Assert.That(Current().Outcome(sheet, ("injuryRoll", 3), ("injuryLocationRoll", 3), ("locationType", 0)), Is.True);

            sheet = TestSystem.RoundTrip(sheet);
            Current().Complete();
            sheet.Time.Refresh();

            sheet = TestSystem.RoundTrip(sheet);
            sheet.Time.ToTurn(2);

            sheet = TestSystem.RoundTrip(sheet);
            var pc = (sheet.Actor as PlayerCharacter)!;

            Assert.That(pc.CurrentStaminaPoints, Is.EqualTo(0));
            Assert.That(pc.CurrentLifePoints, Is.EqualTo(4));
            Assert.That(pc.LeftArm.InjurySeverity, Is.EqualTo(5));
            Assert.That(pc.LeftArm.Injuries.Count(), Is.EqualTo(1));
            Assert.That(pc.LeftArm.Injuries[0].Severity, Is.EqualTo(5));
            Assert.That(pc.IsStateOn(nameof(Exhausted)), Is.True);
        }
    }
}
