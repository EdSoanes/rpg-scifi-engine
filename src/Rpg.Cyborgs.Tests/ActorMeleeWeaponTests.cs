using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;
using Rpg.ModObjects.Reflection.Args;

namespace Rpg.Cyborgs.Tests
{
    public class ActorMeleeWeaponTests
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
        public void Sword_EnsureValues()
        {
            Assert.That(_sword, Is.Not.Null);
            Assert.That(_sword.Name, Is.EqualTo("Excalibur"));
            Assert.That(_sword.Damage.ToString(), Is.EqualTo("1d6"));
            Assert.That(_sword.HitBonus, Is.EqualTo(1));
        }

        [Test]
        public void PlayerCharacter_CarryingSword_Attack()
        {
            _graph.Time.Transition(PointInTimeType.EncounterBegins);

            var activity = _pc.InitiateAction(_sword, nameof(MeleeAttack));
            var attack = activity.CurrentAction()!;

            Assert.That(_graph.Time.Now.Type, Is.EqualTo(PointInTimeType.Turn));
            Assert.That(_graph.Time.Now.Count, Is.EqualTo(1));

            activity.Cost(activity.CostArgs());

            Assert.That(attack.CostModSet.Mods.Count(), Is.EqualTo(1));
            _graph.AddModSets(attack.CostModSet);
            attack.CostModSet.Apply();
            _graph.Time.TriggerEvent();

            Assert.That(_pc.CurrentActionPoints, Is.EqualTo(_pc.ActionPoints - 1));

            var performArgs = activity.PerformArgs();
            performArgs.Set("targetDefence", 12);
            activity.Perform(performArgs);

            Assert.That(attack.Value("diceRoll").ToString(), Is.EqualTo("2d6 + 1"));
            Assert.That(attack.Value("targetDefence").ToString(), Is.EqualTo("12"));

            var outcomeArgs = activity.OutcomeArgs();
            outcomeArgs.Set("diceRoll", 14);
            outcomeArgs.Set("targetDefence", 12);

            var outcomeResult = activity.Outcome(outcomeArgs);
            Assert.That(attack.Value("damage").ToString(), Is.EqualTo("1d6"));

            activity.Complete();

        }
    }
}
