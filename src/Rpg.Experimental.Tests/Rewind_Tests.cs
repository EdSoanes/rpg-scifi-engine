using Rpg.Experimental.Json;
using Rpg.Experimental.Mods;
using Rpg.Experimental.Reflection;
using Rpg.Experimental.Tests.Models;
using Temporal = Rpg.Experimental.Mods.Temporal;

namespace Rpg.Experimental.Tests
{
    /// <summary>
    /// Going back to the start of an earlier turn while turns are being tracked
    /// </summary>
    public class Rewind_Tests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeUtilities.RegisterAssembly(typeof(TestObject).Assembly);
        }

        private static RpgActivityAction Strike(RpgCharacterSheet characterSheet, TestCreature creature, int effectTurns = 0)
        {
            var activity = characterSheet.CreateActivity(creature.Id, creature.Id, nameof(TestStrike));
            var strike = activity.CurrentActivityAction!;

            Assert.That(strike.Cost(characterSheet), Is.True);
            Assert.That(strike.Outcome(characterSheet, ("effectTurns", effectTurns), ("trivial", 0)), Is.True);

            strike.Complete(characterSheet);
            return strike;
        }

        private static TestCreature Creature(RpgCharacterSheet characterSheet, string id)
            => (characterSheet.GetObject(id) as TestCreature)!;

        [Test]
        public void Rewind_OutsideTurnTracking_NothingToGoBackTo()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            Assert.That(characterSheet.GetRewindableTurns(), Is.Empty);
            Assert.That(characterSheet.RewindToTurn(1), Is.False);
        }

        [Test]
        public void Rewind_EveryTurnCanBeGoneBackTo()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.BeginTurnTracking();
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1 }));

            characterSheet.AdvanceToTurn(4);
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2, 3, 4 }));

            Assert.That(characterSheet.RewindToTurn(7), Is.False);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(4));
        }

        [Test]
        public void Rewind_RestoresEverything_AndDiscardsLaterTurns()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            //Turn 1: an action with a 3 turn effect
            Strike(characterSheet, creature, effectTurns: 3);

            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            //Turn 2: another action, a value changed by hand, a state switched on by hand, a permanent change
            characterSheet.NextTurn();

            Strike(characterSheet, creature);
            characterSheet.Add(new Override(), creature, x => x.Health, 9);
            characterSheet.Add(new Standard(), creature, x => x.Bonus, 10);
            characterSheet.GetObjectState(id, nameof(TestBleeding))!.UserEnabled();
            characterSheet.Time.Refresh();

            Assert.That(creature.Ammo, Is.EqualTo(8));
            Assert.That(creature.Health, Is.EqualTo(9));
            Assert.That(creature.Bonus, Is.EqualTo(12));
            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.True);

            //Turn 3
            characterSheet.NextTurn();
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2, 3 }));

            //Back to the start of turn 2: turn 1's action stands, nothing from turn 2 has happened
            Assert.That(characterSheet.RewindToTurn(2), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(characterSheet.Time.IsTurnTracking, Is.True);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2 }));
            Assert.That(characterSheet.Actor.Id, Is.EqualTo(id));

            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Health, Is.EqualTo(5));
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.IsStateOn(nameof(TestBleeding)), Is.False);
            Assert.That(characterSheet.GetObjectState(id, nameof(TestBleeding))!.IsUserEnabled, Is.Null);

            //Play on from there. The effect from turn 1 still runs its course.
            Strike(characterSheet, creature);
            Assert.That(creature.Ammo, Is.EqualTo(8));
            Assert.That(creature.Actions, Is.EqualTo(0));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(2));
            Assert.That(creature.Actions, Is.EqualTo(1));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2, 3, 4 }));

            //And all the way back to the beginning
            Assert.That(characterSheet.RewindToTurn(1), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(10));
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1 }));
        }

        [Test]
        public void Rewind_ToCurrentTurn_UndoesTheTurnSoFar()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();

            Strike(characterSheet, creature, effectTurns: 2);
            Assert.That(creature.Ammo, Is.EqualTo(9));

            Assert.That(characterSheet.RewindToTurn(2), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(2));
            Assert.That(creature.Ammo, Is.EqualTo(10));
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(0));
            Assert.That(characterSheet.GetOwnerObjects<RpgActivity>(id), Is.Empty);
        }

        [Test]
        public void Rewind_ActionThatStartedTurnTracking_IsPartOfTurnOne()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);

            //The effect starts turn tracking
            Strike(characterSheet, creature, effectTurns: 2);
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));

            characterSheet.NextTurn();
            Strike(characterSheet, creature);
            Assert.That(creature.Ammo, Is.EqualTo(8));

            Assert.That(characterSheet.RewindToTurn(1), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(1));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Actions, Is.EqualTo(0));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.NextTurn();
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void Rewind_MovedItem_GoesBack()
        {
            var parent = new TestObject("Parent");
            var child = new TestObject("Child");
            parent.Child = child;

            var characterSheet = new RpgCharacterSheet(parent);
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();

            characterSheet.Move(parent.Id, nameof(TestObject.Children), child.Id);
            characterSheet.Time.Refresh();

            Assert.That(parent.Child, Is.Null);
            Assert.That(parent.Children.Count, Is.EqualTo(1));

            characterSheet.NextTurn();
            Assert.That(characterSheet.RewindToTurn(2), Is.True);

            var parent2 = (characterSheet.GetObject(parent.Id) as TestObject)!;

            Assert.That(parent2.Child, Is.Not.Null);
            Assert.That(parent2.Child.Id, Is.EqualTo(child.Id));
            Assert.That(parent2.Children.Count, Is.EqualTo(0));
        }

        [Test]
        public void Rewind_TimeEventDuringTurnTracking_IsUndone()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);

            characterSheet.Add(new Standard().Until("Sunrise"), creature, x => x.Bonus, 3);
            characterSheet.BeginTurnTracking();
            characterSheet.NextTurn();

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));

            characterSheet.NextTurn();
            Assert.That(characterSheet.RewindToTurn(2), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(creature.Bonus, Is.EqualTo(3));

            characterSheet.TriggerTimeEvent("Sunrise");
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void Rewind_StillWorksAfterSaveAndRestore()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            Strike(characterSheet, creature, effectTurns: 3);
            characterSheet.NextTurn();
            Strike(characterSheet, creature);
            characterSheet.NextTurn();

            var json = RpgJson.SerializeGraphState(characterSheet.GetState());
            var state = RpgJson.DeserializeGraphState<RpgCharacterSheetState>(json);
            var characterSheet2 = new RpgCharacterSheet(state, characterSheet.GetSystem());

            Assert.That(characterSheet2.Time.Turn, Is.EqualTo(3));
            Assert.That(characterSheet2.GetRewindableTurns(), Is.EqualTo(new[] { 1, 2, 3 }));
            Assert.That(Creature(characterSheet2, id).Ammo, Is.EqualTo(8));

            Assert.That(characterSheet2.RewindToTurn(2), Is.True);

            var creature2 = Creature(characterSheet2, id);
            Assert.That(characterSheet2.Time.Turn, Is.EqualTo(2));
            Assert.That(creature2.Ammo, Is.EqualTo(9));
            Assert.That(creature2.Actions, Is.EqualTo(1));
            Assert.That(creature2.Bonus, Is.EqualTo(2));
        }

        [Test]
        public void Rewind_AfterRenumbering_UsesTheNewNumbers()
        {
            var creature = new TestCreature();
            var id = creature.Id;
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            //Active on turns 1, 2 and 3
            Strike(characterSheet, creature, effectTurns: 3);
            characterSheet.NextTurn();
            Strike(characterSheet, creature);
            characterSheet.NextTurn();

            //Turn 3 becomes turn 10
            characterSheet.RenumberTurn(10);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(10));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 8, 9, 10 }));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            Assert.That(characterSheet.RewindToTurn(2), Is.False);
            Assert.That(characterSheet.RewindToTurn(9), Is.True);
            creature = Creature(characterSheet, id);

            Assert.That(characterSheet.Time.Turn, Is.EqualTo(9));
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 8, 9 }));
            Assert.That(creature.Ammo, Is.EqualTo(9));
            Assert.That(creature.Actions, Is.EqualTo(1));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            //The effect still has this turn and the next to run
            characterSheet.NextTurn();
            Assert.That(characterSheet.Time.Turn, Is.EqualTo(10));
            Assert.That(creature.Bonus, Is.EqualTo(2));

            characterSheet.NextTurn();
            Assert.That(creature.Bonus, Is.EqualTo(0));
        }

        [Test]
        public void Rewind_EndingTurnTracking_SettlesIt()
        {
            var creature = new TestCreature();
            var characterSheet = new RpgCharacterSheet(creature);
            characterSheet.BeginTurnTracking();

            Strike(characterSheet, creature);
            characterSheet.AdvanceToTurn(3);
            Assert.That(characterSheet.GetRewindableTurns().Length, Is.EqualTo(3));

            characterSheet.EndTurnTracking();

            Assert.That(characterSheet.GetRewindableTurns(), Is.Empty);
            Assert.That(characterSheet.RewindToTurn(1), Is.False);
            Assert.That(creature.Ammo, Is.EqualTo(9));

            //A new encounter starts its own history
            characterSheet.BeginTurnTracking();
            Assert.That(characterSheet.GetRewindableTurns(), Is.EqualTo(new[] { 1 }));
        }
    }
}
