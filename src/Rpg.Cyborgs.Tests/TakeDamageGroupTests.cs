using Rpg.Cyborgs.ActionGroups;
using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Values;

namespace Rpg.Cyborgs.Tests
{
    public class TakeDamageGroupTests
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

            _graph = new RpgGraph(room);
        }

        [Test]
        public void TakeDamageGroup_CreateActivity_EnsureInstances()
        {
            var activity = _graph.CreateActivity(_pc, new TakeDamageGroup());

            Assert.That(activity, Is.Not.Null);
            Assert.That(activity.ActionInstances.Count, Is.EqualTo(3));
            Assert.That(activity.ActionInstances[0].ActionName == nameof(MeleeParry));
            Assert.That(activity.ActionInstances[1].ActionName == nameof(ArmourCheck));
            Assert.That(activity.ActionInstances[2].ActionName == nameof(TakeDamage));

            Assert.That(activity.ActionInstance, Is.Null);
        }

        [Test]
        public void TakeDamageGroup_SelectInstance_EnsureInstance()
        {
            var activity = _graph.CreateActivity(_pc, new TakeDamageGroup());
            activity.SetActionInstance(nameof(TakeDamage));

            Assert.That(activity.ActionInstance, Is.Not.Null);
            Assert.That(activity.ActionInstance.ActionName, Is.EqualTo(nameof(TakeDamage)));
        }

        [Test]
        public void TakeDamageGroup_TakeDamage_EnsureValues()
        {
            var activity = _graph.CreateActivity(_pc, new TakeDamageGroup());
            activity.SetActionInstance(nameof(TakeDamage));

            activity.Cost();

            var actArgs = activity.GetActArgs();
            actArgs.Values["damage"] = 10;

            activity.SetActArgs(actArgs.Values);

            var doOutcome = activity.Act();
            Assert.That(doOutcome, Is.False);

            activity.Complete();

            Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
        }

        [Test]
        public void TakeDamageGroup_ArmourCheckAndTakeDamage_EnsureValues()
        {
            var activity = _graph
                .CreateActivity(_pc, new TakeDamageGroup())
                .ActivityMod("damage", "Base", 10);

            activity.SetActionInstance(nameof(MeleeParry));
            Assert.That(activity.CanAct(), Is.True);

            activity.Cost();

            var actArgs = activity.GetActArgs();
            actArgs.Values["target"] = 11;
            actArgs.Values["focusPoints"] = 0;

            activity.SetActArgs(actArgs.Values);
            activity.Act();

            var outcomeArgs = activity.GetOutcomeArgs();
            var diceRoll = outcomeArgs.Values["diceRoll"];
            var target = outcomeArgs.Values["target"];

            Assert.That(diceRoll, Is.InstanceOf<Dice>());
            Assert.That(diceRoll.ToString(), Is.EqualTo("2d6 - 1"));
            Assert.That(target, Is.InstanceOf<Int32>());
            Assert.That((int)target, Is.EqualTo(11));

            //activity.SetActionInstance(nameof(TakeDamage));

            //activity.Cost();

            //var actArgs = activity.GetActArgs();
            //actArgs.Values["damage"] = 10;

            //activity.SetActArgs(actArgs.Values);

            //var doOutcome = activity.Act();
            //Assert.That(doOutcome, Is.False);

            //activity.Complete();

            //Assert.That(_pc.CurrentStaminaPoints, Is.EqualTo(4));
        }
    }
}