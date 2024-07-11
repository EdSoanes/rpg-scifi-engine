using Rpg.ModObjects.Actions;
using Rpg.ModObjects.Reflection;
using Rpg.ModObjects.Tests.Actions;
using Rpg.ModObjects.Tests.Models;
using System.Reflection;

namespace Rpg.ModObjects.Tests
{
    public class ModCmdTests
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

            Assert.That(entity.Actions.Count(), Is.EqualTo(1));

            var testAction = entity.GetAction(nameof(TestAction));
            Assert.That(testAction, Is.Not.Null);
            Assert.That(testAction.Name, Is.EqualTo(nameof(TestAction)));
            Assert.That(testAction.OwnerId, Is.EqualTo(entity.Id));

            //Cost
            var costArgSet = testAction.CostArgs();
            Assert.That(costArgSet, Is.Not.Null);
            Assert.That(costArgSet.Count(), Is.EqualTo(2));
            Assert.That(costArgSet[0]!.Name, Is.EqualTo("owner"));
            Assert.That(costArgSet[0]!.TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(costArgSet[1]!.Name, Is.EqualTo("initiator"));
            Assert.That(costArgSet[1]!.TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(costArgSet.ArgNames, Does.Contain("owner"));
            Assert.That(costArgSet.ArgNames, Does.Contain("initiator"));

            //Act
            var actArgSet = testAction.ActArgs();
            Assert.That(actArgSet, Is.Not.Null);
            Assert.That(actArgSet.Count(), Is.EqualTo(4));
            Assert.That(actArgSet[0]!.Name, Is.EqualTo("actionInstance"));
            Assert.That(actArgSet[0]!.TypeName, Is.EqualTo(nameof(ActionInstance)));
            Assert.That(actArgSet[1]!.Name, Is.EqualTo("owner"));
            Assert.That(actArgSet[1]!.TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(actArgSet[2]!.Name, Is.EqualTo("initiator"));
            Assert.That(actArgSet[2]!.TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(actArgSet[3]!.Name, Is.EqualTo("target"));
            Assert.That(actArgSet[3]!.TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(actArgSet.ArgNames, Does.Contain("owner"));
            Assert.That(actArgSet.ArgNames, Does.Contain("initiator"));
            Assert.That(actArgSet.ArgNames, Does.Contain("target"));

            //Outcome
            var outcomeArgs = testAction.OutcomeArgs();
            Assert.That(outcomeArgs, Is.Not.Null);
            Assert.That(outcomeArgs.Count(), Is.EqualTo(3));
            Assert.That(outcomeArgs[0].Name, Is.EqualTo("owner"));
            Assert.That(outcomeArgs[0].TypeName, Is.EqualTo(nameof(ModdableEntity)));
            Assert.That(outcomeArgs[1].Name, Is.EqualTo("initiator"));
            Assert.That(outcomeArgs[1].TypeName, Is.EqualTo(nameof(TestHuman)));
            Assert.That(outcomeArgs[2].Name, Is.EqualTo("diceRoll"));
            Assert.That(outcomeArgs[2].TypeName, Is.EqualTo(nameof(Int32)));
            Assert.That(outcomeArgs.ArgNames, Does.Contain("owner"));
            Assert.That(outcomeArgs.ArgNames, Does.Contain("initiator"));
            Assert.That(outcomeArgs.ArgNames, Does.Contain("diceRoll"));
        }
    }
}
