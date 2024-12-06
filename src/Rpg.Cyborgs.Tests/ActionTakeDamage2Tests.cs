using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Rpg.ModObjects.Reflection.Args;

namespace Rpg.Cyborgs.Tests
{
    public class ActionTakeDamage2Tests
    {
        private RpgGraph _graph;
        private PlayerCharacter _pc;
        private MeleeWeapon _sword;

        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);

            _graph = new RpgGraph(room, _pc);
        }

        [Test]
        public void TakeDamage2_10Damage_Action_EnsureValues()
        {
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));
            
            _graph.Time.Transition(PointInTimeType.EncounterBegins);

            Assert.That(_pc.CanInitiateAction(nameof(TakeDamage)), Is.True);
            var activity = _pc.InitiateAction(nameof(TakeDamage));
            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Active));

            var action = activity.CurrentAction();
            Assert.That(action, Is.Not.Null);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));

            var costArgs = activity.CostArgs();
            Assert.That(costArgs.IsComplete(), Is.True);
            Assert.That(costArgs.Count(), Is.EqualTo(0));

            var costResult = activity.Cost(costArgs);
            Assert.That(costResult, Is.True);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.Started));

            var performArgs = activity.PerformArgs();
            Assert.That(performArgs.IsComplete, Is.True);
            Assert.That(performArgs.Count(), Is.EqualTo(0));

            var performResult = activity.Perform(performArgs);
            Assert.That(performResult, Is.True);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.Started));

            var outcomeArgs = activity.OutcomeArgs();
            Assert.That(outcomeArgs.IsComplete, Is.False);
            Assert.That(outcomeArgs.Count(), Is.EqualTo(3));
            Assert.That(outcomeArgs.Has("damage"), Is.True);

            var damageArg = outcomeArgs.Find("damage");
            Assert.That(damageArg, Is.Not.Null);
            Assert.That(damageArg.Type, Is.EqualTo("Int32"));
            Assert.That(damageArg.Value, Is.Null);

            outcomeArgs.Set("damage", 10);

            var outcomeResult = activity.Outcome(outcomeArgs);
            Assert.That(outcomeResult, Is.True);
            Assert.That(action.OutcomeModSet.Mods.Count(), Is.EqualTo(1));

            Assert.That(action.Status, Is.EqualTo(ActionStatus.CanComplete));

            var nextActions = activity.Complete();
            Assert.That(action.Status, Is.EqualTo(ActionStatus.Completed));
            Assert.That(nextActions.Length, Is.EqualTo(0));

            _graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Expired));  
            Assert.That(action.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(action.CostModSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(action.OutcomeModSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
        }

        [Test]
        public void TakeDamage2_15Damage_NextAction_TakeInjury2()
        {
            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(14));
            Assert.That(_pc.CurrentLifePoints, Is.EqualTo(5));

            _graph.Time.Transition(PointInTimeType.EncounterBegins);

            var activity = _pc.InitiateAction(nameof(TakeDamage));
            var action = activity.CurrentAction();
            Assert.That(action, Is.Not.Null);
            Assert.That(action.Status, Is.EqualTo(ActionStatus.NotStarted));

            var costResult = activity.Cost(activity.CostArgs());
            var performResult = activity.Perform(activity.PerformArgs());

            var outcomeArgs = activity.OutcomeArgs();
            outcomeArgs.Set("damage", 15);
            var outcomeResult = activity.Outcome(outcomeArgs);
            Assert.That(action.OutcomeModSet.Mods.Count(), Is.EqualTo(2));

            var nextActions = activity.Complete();
            Assert.That(nextActions.Length, Is.EqualTo(1));

            var nextAction = nextActions[0];
            Assert.That(nextAction.ActionTemplateName, Is.EqualTo(nameof(TakeInjury)));
            Assert.That(nextAction.ActionTemplateOwnerId, Is.EqualTo(_pc.Id));
            Assert.That(nextAction.Optional, Is.False);

            Assert.That(_pc.CanInitiateAction(nextAction), Is.True);
            _pc.InitiateAction(nextAction);

            var injuryAction = activity.CurrentAction();
            Assert.That(injuryAction, Is.Not.Null);
            Assert.That(injuryAction.Status, Is.EqualTo(ActionStatus.NotStarted));

            var injuryPerformArgs = activity.PerformArgs();
            Assert.That(injuryPerformArgs.IsComplete(), Is.True);
            Assert.That(injuryPerformArgs.Val("lifeInjury"), Is.EqualTo(1));

            var injuryPerformResult = activity.Perform(injuryPerformArgs);
            Assert.That(injuryPerformResult, Is.True);

            Assert.That(injuryAction.Value("injuryRoll").ToString(), Is.EqualTo("2d6 - 1"));
            Assert.That(injuryAction.Value("injuryLocationRoll").ToString(), Is.EqualTo("1d6"));

            var injuryOutcomeArgs = activity.OutcomeArgs();
            injuryOutcomeArgs.Set("injuryRoll", 3);
            injuryOutcomeArgs.Set("injuryLocationRoll", 3); //Left arm
            injuryOutcomeArgs.Set("locationType", 0); //Random location

            var injuryOutcomeResult = activity.Outcome(injuryOutcomeArgs);
            Assert.That(injuryOutcomeResult, Is.True);
            Assert.That(injuryAction.Status, Is.EqualTo(ActionStatus.CanComplete));

            activity.Complete();

            _graph.Time.Transition(PointInTimeType.Turn, 2);
            Assert.That(activity.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(injuryAction.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(injuryAction.CostModSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));
            Assert.That(injuryAction.OutcomeModSet.Expiry, Is.EqualTo(LifecycleExpiry.Expired));

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(0));
            Assert.That(_pc.LeftArm.Injuries.Count(), Is.EqualTo(1));

            var leftArmInjury = _pc.LeftArm.Injuries[0];
            Assert.That(leftArmInjury.Severity, Is.EqualTo(5));
        }
    }
}
