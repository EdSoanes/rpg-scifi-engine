using Rpg.Core.Tests.Models;
using Rpg.ModObjects;
using Rpg.ModObjects.Activities;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Reflection.Args;
using Rpg.ModObjects.Time;
using Rpg.ModObjects.Values;

namespace Rpg.Core.Tests
{
    public class RpgObject_ActionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
            RpgTypeScan.RegisterAssembly(typeof(TestPerson).Assembly);
        }

        [Test]
        public void ActionArgs_CreateFromAction()
        {
            var person = new TestPerson("Benny");
            var weapon = new TestWeapon("Sword");
            person.Hands.Add(weapon);

            var graph = new RpgGraph(person);

            var activity = person.InitiateAction(weapon, nameof(Attack));
            var action = activity.CurrentAction()!;
            var actionArgs = action.ActionArgs;
            
            Assert.That(actionArgs.Count(), Is.EqualTo(5));
            Assert.That(actionArgs.Val("owner"), Is.Not.Null);
            Assert.That(actionArgs.Val("owner"), Is.EqualTo(weapon.Id));
            Assert.That(actionArgs.Val("initiator"), Is.EqualTo(person.Id));
            Assert.That(actionArgs.Val("activity"), Is.Null);
            Assert.That(actionArgs.Val("action"), Is.EqualTo(action.Id));

            Assert.That(actionArgs.Val("targetDefence"), Is.Null);
            Assert.That(actionArgs.Val("diceRoll"), Is.Null);
        }
    }
}
