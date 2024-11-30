using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Reflection.Args;

namespace Rpg.Cyborgs.Tests
{
    public class TransferActionTests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;
        private Room _room;

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            _room = new Room();
            _room.Contents.Add(_pc);

            _graph = new RpgGraph(_room, _pc);
        }

        [Test]
        public void Benny_Transfers_Sword_EnsureInitialValues()
        {
            var activity = _pc.InitiateAction(_sword, nameof(Transfer));
            var action = activity.CurrentAction();

            Assert.That(action, Is.Not.Null);
            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword_OutsideTurn()
        {
            var activity = _pc.InitiateAction(_sword, nameof(Transfer));
            var action = activity.CurrentAction()!;

            var costResult = activity.Cost(activity.CostArgs());
            Assert.That(costResult, Is.True);

            var performArgs = activity.PerformArgs();
            Assert.That(performArgs.IsComplete(), Is.True);

            var performResult = activity.Perform(performArgs);
            Assert.That(performResult, Is.True);

            var outcomeArgs = activity.OutcomeArgs();
            outcomeArgs.Set("from", _pc.Hands);
            outcomeArgs.Set("to", _graph.GetContext<Room>()?.Contents);

            var outcomeResult = activity.Outcome(outcomeArgs);
            Assert.That(outcomeResult, Is.True);

            Assert.That(action.Status, Is.EqualTo(ActionStatus.CanComplete));
            activity.Complete();

            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Drops_Sword_OnTurn2()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(2);

            var activity = _pc.InitiateAction(_sword, nameof(Transfer));
            var action = activity.CurrentAction()!;

            Assert.That(activity.CanAutoComplete, Is.False);

            activity.Cost(activity.CostArgs());

            var performArgs = activity.PerformArgs();
            Assert.That(performArgs.IsComplete(), Is.True);
            activity.Perform(performArgs);

            var outcomeArgs = activity.OutcomeArgs();
            Assert.That(outcomeArgs.IsComplete(), Is.False);
            outcomeArgs.Set("from", _pc.Hands);
            outcomeArgs.Set("to", _graph.GetContext<Room>()?.Contents);
            activity.Outcome(outcomeArgs);
            activity.Complete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.Hands.Contains(_sword), Is.False);
            Assert.That(_room.Contents.Contains(_sword), Is.True);
        }

        [Test]
        public void Benny_Drops_Sword_OnTurn2_RevertToTurn1()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(2);

            var activity = _pc.InitiateAction(_sword, nameof(Transfer));
            var action = activity.CurrentAction()!;

            activity.Cost(activity.CostArgs());
            activity.Perform(activity.PerformArgs());

            var outcomeArgs = activity.OutcomeArgs();
            outcomeArgs.Set("from", _pc.Hands);
            outcomeArgs.Set("to", _graph.GetContext<Room>()?.Contents);
            activity.Outcome(outcomeArgs);
            activity.Complete();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));

            _graph.Time.Transition(1);

            Assert.That(_pc.Hands.Contains(_sword), Is.True);
            Assert.That(_room.Contents.Contains(_sword), Is.False);
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));
        }

        [Test]
        public void Benny_Tries_Two_Transfers_One_Turn()
        {
            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(1));

            _graph.Time.Transition(PointInTimeType.EncounterBegins);

            var dropActivity = _pc.InitiateAction(_sword, nameof(Transfer));
            var dropAction = dropActivity.CurrentAction()!;

            dropActivity.AutoComplete(
                ("from", _pc.Hands),
                ("to", _graph.GetContext<Room>()?.Contents)
            );

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(0));
            Assert.That(_pc.CanInitiateAction(_sword, nameof(Transfer)), Is.False);

            //var pickupActivity = _pc.InitiateAction(_sword, nameof(Transfer));
            //var pickupAction = pickupActivity.CurrentAction()!;

            //var canPerformPickupArgs = pickupActivity.CanPerformArgs();
            //canPerformPickupArgs.Set("to", _pc.Hands.Contents);
            //var canPickup = pickupActivity.CanPerform(canPerformPickupArgs);

            //Assert.That(canPickup, Is.False);

            //pickupActivity.Cost(dropActivity.CostArgs());
            //pickupActivity.Perform(dropActivity.PerformArgs());
            //var pickupOutcomeArgs = pickupActivity.OutcomeArgs();
            //pickupOutcomeArgs.Set("from", (_graph.Context as Room).Contents);
            //pickupActivity.Outcome(outcomeArgs);

        }
    }
}
