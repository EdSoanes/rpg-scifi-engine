using Rpg.Cyborgs.Actions;
using Rpg.Cyborgs.States;
using Rpg.Cyborgs.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

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
            RpgReflection.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new RpgContainer("Room");
            room.Add(_pc);

            _graph = new RpgGraph(room);
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
            var attack = _sword.CreateActionInstance(_pc, nameof(MeleeAttack), 0)!;

            _graph.Time.SetTime(TimePoints.BeginningOfEncounter);
            Assert.That(_graph.Time.Current, Is.EqualTo(TimePoints.Encounter(1)));

            attack.CostArgs["focusPoints"] = 0;
            var cost = attack.Cost(_graph);

            Assert.That(_pc.CurrentActions, Is.EqualTo(1));

            _pc.AddModSet(cost);
            _graph.Time.TriggerEvent();
            Assert.That(_pc.CurrentActions, Is.EqualTo(0));

            attack.ActArgs["focusPoints"] = 0;
            var actionModSet = attack.Act(_graph);
            _pc.AddModSets(actionModSet);
            _graph.Time.TriggerEvent();
            Assert.That(actionModSet.DiceRoll(_graph).ToString(), Is.EqualTo("2d6 - 1"));

            _graph.Time.SetTime(TimePoints.Encounter(2));
            Assert.That(_pc.CurrentActions, Is.EqualTo(1));
            Assert.That(actionModSet.DiceRoll(_graph).ToString(), Is.EqualTo(Dice.Zero.ToString()));
        }
    }
}
