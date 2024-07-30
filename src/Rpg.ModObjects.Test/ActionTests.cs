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
            RpgReflection.RegisterAssembly(this.GetType().Assembly);
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
            var costArgSet = testAction.CostArgs();
            Assert.That(costArgSet, Is.Not.Null);
            Assert.That(costArgSet.Args.Count(), Is.EqualTo(2));
            Assert.That(costArgSet.Args.ContainsKey("owner"), Is.True);
            Assert.That(costArgSet.Args["owner"].TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(costArgSet.Args.ContainsKey("initiator"), Is.True);
            Assert.That(costArgSet.Args["initiator"].TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(costArgSet.Args.Keys, Does.Contain("owner"));
            Assert.That(costArgSet.Args.Keys, Does.Contain("initiator"));

            //Act
            var actArgSet = testAction.ActArgs();
            Assert.That(actArgSet, Is.Not.Null);
            Assert.That(actArgSet.Args.Count(), Is.EqualTo(4));
            Assert.That(actArgSet.Args.ContainsKey("actionInstance"), Is.True);
            Assert.That(actArgSet.Args["actionInstance"].TypeName, Is.EqualTo(nameof(ActionInstance)));
            Assert.That(actArgSet.Args.ContainsKey("owner"), Is.True);
            Assert.That(actArgSet.Args["owner"].TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(actArgSet.Args.ContainsKey("initiator"), Is.True);
            Assert.That(actArgSet.Args["initiator"].TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(actArgSet.Args.ContainsKey("target"), Is.True);
            Assert.That(actArgSet.Args["target"].TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(actArgSet.Args.Keys, Does.Contain("owner"));
            Assert.That(actArgSet.Args.Keys, Does.Contain("initiator"));
            Assert.That(actArgSet.Args.Keys, Does.Contain("target"));

            //Outcome
            var outcomeArgs = testAction.OutcomeArgs();
            Assert.That(outcomeArgs, Is.Not.Null);
            Assert.That(outcomeArgs.Args.Count(), Is.EqualTo(3));
            Assert.That(actArgSet.Args.ContainsKey("owner"), Is.True);
            Assert.That(actArgSet.Args["owner"].TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(actArgSet.Args.ContainsKey("initiator"), Is.True);
            Assert.That(actArgSet.Args["initiator"].TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(outcomeArgs.Args.ContainsKey("diceRoll"), Is.True);
            Assert.That(outcomeArgs.Args["diceRoll"].TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(outcomeArgs.Args.Keys, Does.Contain("owner"));
            Assert.That(outcomeArgs.Args.Keys, Does.Contain("initiator"));
            Assert.That(outcomeArgs.Args.Keys, Does.Contain("diceRoll"));
        }
    }
}
