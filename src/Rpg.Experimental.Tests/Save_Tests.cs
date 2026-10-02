using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Temporal = Rpg.Experimental.Mods.Temporal;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// Saving a sheet as small text: compressed, without indentation, without anything a restore gets right
    /// without being told, and with a bounded turn history.
    /// </summary>
    public class Save_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static string Json(RpgCharacterSheet characterSheet)
            => RpgJson.SerializeGraphState(characterSheet.GetState());

        private static RpgCharacterSheet RoundTrip(RpgCharacterSheet characterSheet)
            => RpgCharacterSheet.Load(characterSheet.Save(), characterSheet.GetSystem());

        #region Size

        [Test]
        public void Save_IsCompressed_AndRestoresTheSameSheet()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var json = Json(characterSheet);
            var saved = characterSheet.Save();

            Assert.That(saved.Length, Is.LessThan(json.Length / 3));
            Assert.That(saved.TrimStart().StartsWith('{'), Is.False);

            var characterSheet2 = RpgCharacterSheet.Load(saved, characterSheet.GetSystem());

            Assert.That(Json(characterSheet2), Is.EqualTo(json));
            Assert.That((characterSheet2.Actor as TestCreature)!.Health, Is.EqualTo(5));
        }

        [Test]
        public void Load_AcceptsTextThatIsNotCompressed()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var characterSheet2 = RpgCharacterSheet.Load(Json(characterSheet), characterSheet.GetSystem());

            Assert.That(Json(characterSheet2), Is.EqualTo(Json(characterSheet)));
        }

        [Test]
        public void Json_IsNotIndented_AndLeavesOutWhatARestoreAlreadyKnows()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            var json = Json(characterSheet);

            Assert.That(json, Does.Not.Contain("\n"));
            Assert.That(json, Does.Not.Contain("\"isLifespanRelative\":false"));
            Assert.That(json, Does.Not.Contain("\"isApplied\":true"));
            Assert.That(json, Does.Not.Contain("\"isUserEnabled\":null"));
            Assert.That(json, Does.Not.Contain("\"startsTurnTracking\":true"));
            Assert.That(json, Does.Not.Contain("\"modIds\":[]"));
        }

        #endregion Size

        #region Nothing is lost

        [Test]
        public void Restore_KeepsValuesThatDifferFromWhatANewObjectHas()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking(3);

            //Each of these sets something to the opposite of what a newly created object has
            var unapplied = new RpgModSet("Blessing", creature.Id, false);
            unapplied.Add(creature, x => x.Bonus, 3);
            unapplied.Unapply();
            characterSheet.Add(unapplied);

            var bleeding = characterSheet.GetObjectState(creature.Id, nameof(TestBleeding))!;
            bleeding.UserDisabled();

            var minor = new Temporal(2).NoTurnTracking().SetName("Minor");
            characterSheet.Add(minor, creature, x => x.Bonus, 1);

            var manual = characterSheet.OverrideByHand(creature, x => x.Health, 1);
            characterSheet.RequestRoll(creature, "check", "2d6");
            characterSheet.SetRoll(creature, "check", 14);
            characterSheet.RollMode = RpgRollMode.App;
            characterSheet.MaxTurnHistory = 3;
            characterSheet.NextTurn();

            Assert.That(creature.Bonus, Is.EqualTo(1));
            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.False);

            var characterSheet2 = RoundTrip(characterSheet);
            var creature2 = (characterSheet2.Actor as TestCreature)!;

            //The same text again, so nothing was dropped on the way
            Assert.That(Json(characterSheet2), Is.EqualTo(Json(characterSheet)));

            Assert.That(characterSheet2.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet2.Time.Turn, Is.EqualTo(4));
            Assert.That(characterSheet2.RollMode, Is.EqualTo(RpgRollMode.App));
            Assert.That(characterSheet2.MaxTurnHistory, Is.EqualTo(3));

            var unapplied2 = (characterSheet2.GetLifecycleObject(unapplied.Id) as RpgModSet)!;
            Assert.That(unapplied2.IsApplied, Is.False);
            Assert.That(unapplied2.Name, Is.EqualTo("Blessing"));
            Assert.That(unapplied2.Mods.Length, Is.EqualTo(1));
            Assert.That(unapplied2.Mods[0].IsApplied, Is.False);

            var bleeding2 = characterSheet2.GetObjectState(creature2.Id, nameof(TestBleeding))!;
            Assert.That(bleeding2.IsUserEnabled, Is.False);
            Assert.That(bleeding2.NeedsTurnTracking, Is.True);
            Assert.That(creature2.IsStateOn(nameof(TestBleeding)), Is.False);

            var minor2 = characterSheet2.GetMods([minor.Id]).Single();
            Assert.That(minor2.StartsTurnTracking, Is.False);
            Assert.That(minor2.Name, Is.EqualTo("Minor"));
            Assert.That(minor2.End, Is.EqualTo(minor.End));

            var manual2 = characterSheet2.GetManualChanges().Single();
            Assert.That(manual2.Id, Is.EqualTo(manual.Id));
            Assert.That(creature2.Health, Is.EqualTo(1));

            var roll2 = characterSheet2.GetRoll(creature2.Id, "check")!;
            Assert.That(roll2.Result, Is.EqualTo(14));
            Assert.That(roll2.IsOutOfRange, Is.True);
            Assert.That(roll2.SuppliedBy, Is.EqualTo(RpgRollSource.Player));

            //And it carries on working: the minor effect ends when it should
            characterSheet2.NextTurn();
            Assert.That(creature2.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void Restore_KeepsValuesSetBackToZeroOrNothing()
        {
            var obj = new TestObject("Thing");
            var characterSheet = new RpgCharacterSheet(obj);

            //Strength starts at 10 and Intelligence at 3. Both are taken down to nothing.
            characterSheet.OverrideByHand(obj, x => x.Strength, 0);
            characterSheet.OverrideByHand(obj, x => x.Intelligence, 0);

            Assert.That(obj.Strength, Is.EqualTo(0));

            var characterSheet2 = RoundTrip(characterSheet);
            var obj2 = (characterSheet2.Actor as TestObject)!;

            Assert.That(obj2.Strength, Is.EqualTo(0));
            Assert.That(obj2.Intelligence, Is.EqualTo(0));
            Assert.That(obj2.StrengthBonus, Is.EqualTo(-5));
            Assert.That(Json(characterSheet2), Is.EqualTo(Json(characterSheet)));
        }

        #endregion Nothing is lost

        #region Turn history

        [Test]
        public void TurnHistory_DefaultsToFiveTurns()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature);

            Assert.That(characterSheet.MaxTurnHistory, Is.EqualTo(5));
            Assert.That(RpgCharacterSheet.DefaultMaxTurnHistory, Is.EqualTo(5));

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(3);
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2, 3 }));

            characterSheet.AdvanceToTurn(8);
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 4, 5, 6, 7, 8 }));

            //A turn that has been dropped cannot be gone back to, and nothing changes
            Assert.That(characterSheet.RewindToTurn(3), Is.False);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(8));

            Assert.That(characterSheet.RewindToTurn(4), Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(4));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 4 }));
        }

        [Test]
        public void TurnHistory_CanBeChanged()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature) { MaxTurnHistory = 2 };

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(6);
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 5, 6 }));

            //More from now on. Turns already dropped do not come back.
            characterSheet.MaxTurnHistory = 10;
            characterSheet.AdvanceToTurn(9);
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));

            //Fewer takes effect straight away
            characterSheet.MaxTurnHistory = 1;
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 9 }));
        }

        [Test]
        public void TurnHistory_Zero_KeepsNothing()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature) { MaxTurnHistory = 0 };

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(4);

            Assert.That(characterSheet.GetRewindableTurns(), Is.Empty);
            Assert.That(characterSheet.RewindToTurn(4), Is.False);

            characterSheet.MaxTurnHistory = -3;
            Assert.That(characterSheet.MaxTurnHistory, Is.EqualTo(0));
        }

        [Test]
        public void TurnHistory_SettingIsSaved_AndNotRewound()
        {
            var creature = new TestCreature("Rat");
            var characterSheet = new RpgCharacterSheet(creature) { MaxTurnHistory = 3 };

            characterSheet.BeginTurnTracking();
            characterSheet.AdvanceToTurn(6);

            var characterSheet2 = RoundTrip(characterSheet);
            Assert.That(characterSheet2.MaxTurnHistory, Is.EqualTo(3));
            Assert.That(characterSheet2.GetRewindableTurns(), Is.EqualTo(new[] { 4, 5, 6 }));

            characterSheet2.MaxTurnHistory = 8;
            Assert.That(characterSheet2.RewindToTurn(5), Is.True);
            Assert.That(characterSheet2.MaxTurnHistory, Is.EqualTo(8));
        }

        [Test]
        public void TurnHistory_BoundsTheSizeOfALongFight()
        {
            int SavedSizeAfter40Turns(int maxTurnHistory)
            {
                var creature = new TestCreature("Rat");
                var characterSheet = new RpgCharacterSheet(creature) { MaxTurnHistory = maxTurnHistory };
                characterSheet.BeginTurnTracking();
                characterSheet.AdvanceToTurn(40);

                Assert.That(characterSheet.GetRewindableTurns().Length, Is.EqualTo(Math.Min(maxTurnHistory, 40)));
                return characterSheet.Save().Length;
            }

            var keepingFive = SavedSizeAfter40Turns(5);
            var keepingAll = SavedSizeAfter40Turns(100);

            Assert.That(keepingFive, Is.LessThan(keepingAll * 0.7));
        }

        #endregion Turn history
    }
}
