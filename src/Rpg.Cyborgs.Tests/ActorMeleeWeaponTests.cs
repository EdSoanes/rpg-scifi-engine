using Rpg.Cyborgs.Actions;
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
            RpgTypeScan.RegisterAssembly(typeof(CyborgsSystem).Assembly);

            _sword = new MeleeWeapon(WeaponFactory.SwordTemplate);
            _pc = new PlayerCharacter(ActorFactory.BennyTemplate);
            _pc.Hands.Add(_sword);

            var room = new Room();
            room.Contents.Add(_pc);

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
            var activity = new RpgActivity(_pc, 0);
            _graph.AddEntity(activity);

            activity.CreateActionInstance(_sword, nameof(MeleeAttack));
            var attack = activity.ActionInstance;

            _graph.Time.SetTime(TimePoints.BeginningOfEncounter);
            Assert.That(_graph.Time.Current, Is.EqualTo(TimePoints.Encounter(1)));

            activity.Cost();
            Assert.That(activity.OutcomeSet.Mods.Count(), Is.EqualTo(1));

            activity
                .SetActArg("focusPoints", 0)
                .SetActArg("targetDefence", 12)
                .Act();

            Assert.That(activity.GetActionProp("diceRoll")?.ToString(), Is.EqualTo("2d6"));
            Assert.That(activity.GetActionProp("target")?.ToString(), Is.EqualTo("12"));

            activity
                .ActionResultMod("diceRoll", "Result", 14)
                .ActionResultMod("target", "Result", 12);

            Assert.That(activity.GetActionProp("diceRoll")?.ToString(), Is.EqualTo("14"));
            Assert.That(activity.GetActionProp("target")?.ToString(), Is.EqualTo("12"));
            
            activity.Outcome();
            Assert.That(activity.GetActivityProp("damage")?.ToString(), Is.EqualTo("1d6 - 1"));

            activity.Complete();

        }
    }
}
