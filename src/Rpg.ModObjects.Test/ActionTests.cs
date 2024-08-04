using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Actions;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ActionTests
    {
        [SetUp]
        public void Setup()
        {
            RpgTypeScan.RegisterAssembly(this.GetType().Assembly);
        }

        [Test]
        public void ModdableEntity_EnsureSetup()
        {
            var entity = new ModdableEntity();
            var graph = new RpgGraph(entity);
            //var activity = new RpgActivity(graph, entity, 0);
            //graph.AddEntity(activity);

            Assert.That(entity.Actions.Count(), Is.EqualTo(1));

            var testAction = entity.GetAction(nameof(TestAction));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.Name, Is.EqualTo(nameof(TestAction)));
            Assert.That(testAction.OwnerId, Is.EqualTo(entity.Id));

            //Cost
            var costArgs = testAction.OnCost.Args;
            Assert.That(costArgs, Is.Not.Null);
            Assert.That(costArgs.Count(), Is.EqualTo(3));
            Assert.That(costArgs.FirstOrDefault(x => x.Name == "owner"), Is.Not.Null);
            Assert.That(costArgs.First(x => x.Name == "owner").TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(costArgs.FirstOrDefault(x => x.Name == "initiator"), Is.Not.Null);
            Assert.That(costArgs.First(x => x.Name == "initiator").TypeName, Is.EqualTo(nameof(TestHuman)));

            //Act
            var actArgs = testAction.OnAct.Args;
            Assert.That(actArgs, Is.Not.Null);
            Assert.That(actArgs.Count(), Is.EqualTo(4));
            Assert.That(actArgs.FirstOrDefault(x => x.Name == "activity"), Is.Not.Null);
            Assert.That(actArgs.First(x => x.Name == "activity").TypeName, Is.EqualTo(nameof(Activity)));
            Assert.That(actArgs.FirstOrDefault(x => x.Name == "owner"), Is.Not.Null);
            Assert.That(actArgs.First(x => x.Name == "owner").TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(actArgs.FirstOrDefault(x => x.Name == "initiator"), Is.Not.Null);
            Assert.That(actArgs.First(x => x.Name == "initiator").TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(actArgs.FirstOrDefault(x => x.Name == "target"), Is.Not.Null);
            Assert.That(actArgs.First(x => x.Name == "target").TypeName, Is.EqualTo(nameof(Int32)));

            //Outcome
            var outcomeArgs = testAction.OnOutcome.Args;
            Assert.That(outcomeArgs, Is.Not.Null);
            Assert.That(outcomeArgs.Count(), Is.EqualTo(4));
            Assert.That(outcomeArgs.FirstOrDefault(x => x.Name == "owner"), Is.Not.Null);
            Assert.That(outcomeArgs.First(x => x.Name == "owner").TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(outcomeArgs.FirstOrDefault(x => x.Name == "initiator"), Is.Not.Null);
            Assert.That(outcomeArgs.First(x => x.Name == "initiator").TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(outcomeArgs.FirstOrDefault(x => x.Name == "diceRoll"), Is.Not.Null);
            Assert.That(outcomeArgs.First(x => x.Name == "diceRoll").TypeName, Is.EqualTo(nameof(Int32)));
        }
    }
}
